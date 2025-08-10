namespace BruSoftware.SharedServices.Interfaces;

public interface ISpeechService
{
    void Speak(string text);
    void SpeakAsync(string text);
}