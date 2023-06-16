namespace MauiTubePlayer.Models;

public static class Constants
{
    public static string ApplicationName = "MauiTubePlayer";
    public static string EmailAddress = @"maid.dubravac@fsk.unsa.ba";
    public static string ApplicationId = "RAMU.MauiTubePlayer.App";
    public static string ApiServiceURL = @"https://youtube.googleapis.com/youtube/v3/";
    public static string ApiKey = @"AIzaSyA8A5_S4jkdUG38kG9a1nvq6jgNhNYYouw";


    public static uint MicroDuration { get; set; } = 100;
    public static uint SmallDuration { get; set; } = 300;
    public static uint MediumDuration { get; set; } = 600;
    public static uint LongDuration { get; set; } = 1200;
    public static uint ExtraLongDuration { get; set; } = 1800;
}