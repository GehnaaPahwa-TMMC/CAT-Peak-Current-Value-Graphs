using SQLiteData;
using Graph;
class Program
{
    static void Main(string[] args)
    {
        string connectionString = "Data Source=C:\\Users\\pahwag\\OneDrive - TMNA\\Desktop\\cellmate.sqlite";

        // Collect SQLite Data
        Dictionary<string, Dictionary<int, List<Entry>>> currVals = CurrentValues.CollectData(connectionString);
        Console.WriteLine("SQLite Data collected.");

        // Create directory
        string path = $"C:\\Users\\pahwag\\OneDrive - TMNA\\Desktop\\GeneratedGraphs";
        string currentDate = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
        string fullFolderPath = Path.Combine(path, currentDate);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        Directory.CreateDirectory(fullFolderPath);

        // Generate Graphs
        foreach (KeyValuePair<string, Dictionary<int, List<Entry>>> robot in currVals)
        {
            CurrentValuesGraph.GenerateGraph(robot.Key, robot.Value, fullFolderPath);
        }
        Console.WriteLine("Graphs created.");
    }
}