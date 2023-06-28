namespace YTMUSIC;

public class Model{
    public string ID {get;set;}
    public string Title {get;set;}
    public string Author {get;set;}
    public string url {get;set;}
    public string Thumbnails {get;set;}

    public Model(string ID, string Title, string Author, string url, string Thumbnails){
        this.ID = ID;
        this.Title = Title;
        this.Author = Author;
        this.url = url;
        this.Thumbnails = Thumbnails;
    }
}