namespace NetShared.Net
{
    public interface ISerializer
    {
        T Deserialize<T>(byte[] bytes, int index, int count);
        byte[] Serialize(object obj);
    }
}