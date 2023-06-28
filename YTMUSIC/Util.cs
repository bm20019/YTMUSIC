using System.Text.RegularExpressions;
using LibVLCSharp.Shared;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YTMUSIC;

public class Util
{
    private double Tiempo = 0L;
    private string Title = "", descripcion = "";
    private bool terminal = false;
    private List<Model> listaplayed;
    ConsoleKeyInfo keyInfo;
    int contador = 0;
    YoutubeClient youtubeClient;
    LibVLC libVLC;
    MediaPlayer mediaPlayer;
    const int cuantosSeparadores = 90;
    public Util()
    {
        libVLC = new LibVLC(enableDebugLogs: false);
    }

    public void PlayMusicPlaylist(string url)
    {
        Console.WriteLine("Ingresa la url  de la playlist:");
        string urlPlaylist = String.IsNullOrEmpty(url) ? Console.ReadLine() : url;
        if (isValidPlaylist(urlPlaylist))
        {
            this.listaplayed = GetPlaylistUrl(urlPlaylist).Result;
            PrintFor('-', cuantosSeparadores);
            //Imprime la lista de pistas
            for (int i = 0; i < this.listaplayed.Count; i++)
            {
                var item = this.listaplayed[i];
                Console.WriteLine("{0}) {1} | {2}", i + 1, item.Title, item.Author);
            }
            PrintFor('-', cuantosSeparadores);
            //Empieza a reproducir las pistas
            for (int i = 0; i < this.listaplayed.Count; i++)
            {
                this.contador = i;
                played(this.listaplayed[i].url);
                i = this.contador;
            }
        }
    }

    private async Task<List<Model>> GetPlaylistUrl(String urlPlaylist)
    {
        var youtube = new YoutubeClient();
        List<Model> mlist = new List<Model>();
        await foreach (var video in youtube.Playlists.GetVideosAsync(urlPlaylist))
        {
            mlist.Add(new Model(video.Id.Value,
            video.Title,
            video.Author.ChannelTitle,
            "https://music.youtube.com/watch?v=" + video.Id.Value,
            video.Thumbnails[0].Url));
        }
        this.listaplayed = mlist;
        return mlist;
    }
    private void played(String URLPlayed)
    {
        youtubeClient = new YoutubeClient();
        var info = youtubeClient.Videos.GetAsync(URLPlayed).Result;
        var streamManifest = youtubeClient.Videos.Streams.GetManifestAsync(URLPlayed).Result;
        var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        using var media = new Media(libVLC, new Uri(streamInfo.Url));
        mediaPlayer = new MediaPlayer(media);
        //Eventos
        mediaPlayer.TimeChanged += MediaplayerOnTimeChanged;
        mediaPlayer.EndReached += MediaplayerOnEndReached;
        mediaPlayer.EncounteredError += MediaplayerOnEncounteredError;
        //Informacion
        Tiempo = info.Duration.Value.TotalSeconds;
        Title = info.Title;
        descripcion = info.Author.ChannelTitle;
        Console.WriteLine("{2}) {0} | {1}", Title, descripcion, this.contador + 1);
        mediaPlayer.Play();
        Console.TreatControlCAsInput = true;
        terminal = false;

        while (terminal == false)
        {
            if (terminal == true)
            {
                mediaPlayer.Stop();
                Console.WriteLine("FASLE");
                terminal = true;
                break;
            }
            if (Console.KeyAvailable)
            {
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.P)
                {
                    if (mediaPlayer.IsPlaying)
                        mediaPlayer.Pause();
                    else
                        mediaPlayer.Play();
                }
                else if (keyInfo.Key == ConsoleKey.F)
                {//Siguiente Pista
                    mediaPlayer.Stop();
                    terminal = true;
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.R)
                {
                    terminal = true;
                    bool isnumber = false;
                    do
                    {
                        Console.WriteLine("Que Nº de pista desear reproducir?{0}", Environment.NewLine);
                        string index = Console.ReadLine();
                        try
                        {
                            int number = int.Parse(index);
                            if (number >= listaplayed.Count || number <= 0)
                            {
                                Console.WriteLine("Fuera de rango debe estar entre 1/{0}", listaplayed.Count);
                                isnumber = false;
                            }
                            else
                            {
                                mediaPlayer.Stop();
                                terminal = true;
                                played(listaplayed[number - 1].url);
                                contador = number - 1;
                                isnumber = true;
                            }
                        }
                        catch (FormatException ex)
                        {
                            isnumber = true;
                        }
                    } while (isnumber == false);
                }
                else if (keyInfo.Key == ConsoleKey.End)
                {//Cerrar reproductor
                    Environment.Exit(0);
                }
            }
            Thread.Sleep(100);
        }
    }

    private void MediaplayerOnTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
    {
        TimeSpan tiempoActual = TimeSpan.FromMilliseconds(e.Time);
        int percent = (int)(((double)tiempoActual.TotalSeconds / (double)Tiempo) * 100);
        double barLength = percent /1;
        Console.Write($"[{new string('#', (int)barLength)}{new string('_', 100 - (int)barLength)}] {percent}% || {Convert.ToInt32(tiempoActual.TotalSeconds)}/{Tiempo} \r");
    }

    private void MediaplayerOnEncounteredError(object? sender, EventArgs e)
    {
        terminal = true;
        Console.Error.WriteLine("No se pudo reproducir");
        PrintFor('-', cuantosSeparadores);
    }

    private void MediaplayerOnEndReached(object? sender, EventArgs e)
    {
        terminal = true;
        Console.WriteLine("Pista Finalizada");
        PrintFor('-', cuantosSeparadores);
    }

    private static bool isValidPlaylist(string input)
    {
        Regex regex = new Regex("https://music\\.youtube\\.com/playlist\\?list=", RegexOptions.IgnoreCase);
        return regex.IsMatch(input);
    }

    static void PrintFor(char caracter, int veces)
    {
        for (int i = 0; i < veces; i++)
        {
            Console.Write(caracter);
        }
        Console.WriteLine(Environment.NewLine);
    }
}