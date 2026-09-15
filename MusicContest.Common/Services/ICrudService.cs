namespace MusicContest.Common.Services;

public interface ICrudService<T>
{
    public void Create(T element);
    public T Read(Guid id);
    public IEnumerable<T> ReadAll();
    public void Update(T element);
    public void Remove(T element);
}
