namespace ReversiServer.Assets.Interface
{
    public interface IPasswordValidator
    {
        bool StrongPassword(string password);
    }
}