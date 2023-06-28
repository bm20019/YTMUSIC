namespace YTMUSIC;

public class Program
{
    public static void Main(string[] args)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Youtube Music Console");
            Util ut = new Util();
            
            ut.PlayMusicPlaylist(args[0]);
        }
    }
}