using System.Threading.Tasks;

namespace RippLib.Readability;
public static class TaskExtensions
{
    public static bool HasBeenCanceled(this Task task)
    {
        return task is { IsCanceled: true };
    }
}
