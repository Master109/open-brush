public class Variable<T>
{
    public string name;
    public T value;

    public Variable (string name, T value)
    {
        this.name = name;
        this.value = value;
    }
}