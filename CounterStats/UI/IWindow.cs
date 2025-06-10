namespace CounterStats.UI;

public interface IWindow
{
    //clear the window
    void CleanChildren();
    //refresh the contents of the window
    void Refresh();

    string WindowName { get; }

    string IconName { get; }
}
