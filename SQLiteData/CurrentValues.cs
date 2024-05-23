using System.Data.SQLite;
using Newtonsoft.Json;

namespace SQLiteData
{
    public class CurrentValues
    {
        public static Dictionary<string, Dictionary<int, List<Entry>>> CollectData(string connectionString)
        {
            Dictionary<string, Dictionary<int, List<Entry>>> allData = new Dictionary<string, Dictionary<int, List<Entry>>>();

            // Get peakCurrVals
            List<Row> peakCurrVals = Query("SELECT * FROM events WHERE type='peakCurrVals'", connectionString);

            // Iterate through all peakCurrVals
            foreach (Row peakCurrVal in peakCurrVals)
            {
                // Find values array
                List<double> values = new List<double>();

                foreach (var prop in peakCurrVal.Data.peakCurrVals)
                {
                    dynamic valuesArray = prop.Value.values;

                    if (valuesArray != null)
                    {
                        foreach (double val in valuesArray)
                        {
                            values.Add(val);
                        }
                        break;
                    }
                }

                // Populate allData
                for (int i = 0; i < values.Count; i++)
                {
                    Entry entry = new Entry
                    {
                        Time = peakCurrVal.Time,
                        Value = values[i]
                    };

                    if (!allData.ContainsKey(peakCurrVal.RobotUUID))
                    {
                        allData[peakCurrVal.RobotUUID] = new Dictionary<int, List<Entry>>();
                    }

                    if (!allData[peakCurrVal.RobotUUID].ContainsKey(i))
                    {
                        allData[peakCurrVal.RobotUUID][i] = new List<Entry>();
                    }

                    allData[peakCurrVal.RobotUUID][i].Add(entry);
                }
            }

            // Iterate through allData
            foreach (KeyValuePair<string, Dictionary<int, List<Entry>>> robot in allData)
            {
                // Get anomalies for specific robotUuid
                List<Row> anomalies = Query($"SELECT * FROM events WHERE type='anomalies' AND robotUuid='{robot.Key}' AND data LIKE '%axis%' AND data LIKE '%current%'", connectionString);

                // Iterate through anomalies
                foreach (Row anomaly in anomalies)
                {
                    // Find axis
                    int axis = -1;
                    foreach (var prop in anomaly.Data.anomalies)
                    {
                        dynamic axisNum = prop.Value.axis;

                        if (axisNum != null)
                        {
                            axis = axisNum;
                            break;
                        }
                    }

                    // Find threshold
                    double threshold = 0;
                    foreach (var prop in anomaly.Data.anomalies)
                    {
                        dynamic thresholdNum = prop.Value.threshold;

                        if (thresholdNum != null)
                        {
                            threshold = thresholdNum;
                            break;
                        }
                    }

                    if (axis != -1)
                    {
                        // Populate allData with Threshold for each Entry
                        foreach (Entry entry in robot.Value[axis])
                        {
                            if(entry.Time >= anomaly.Time)
                            {
                                entry.Threshold = threshold;
                            }
                        }
                    }
                }
            }

            return allData;
        }

        // Function for SQL Query
        public static List<Row> Query(string query, string connectionString)
        {
            List<Row> rows = new List<Row>();

            // Create a connection
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                // Open the connection
                connection.Open();

                // Create a command with the query and the connection
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    // Execute the query and get a data reader
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        // Iterate over the results
                        while (reader.Read())
                        {
                            // Read data from the current row
                            string time = reader.GetString(reader.GetOrdinal("time"));
                            string robotUuid = reader.GetString(reader.GetOrdinal("robotUuid"));
                            string data = reader.GetString(reader.GetOrdinal("data"));

                            dynamic jsonObject = JsonConvert.DeserializeObject(data);

                            Row row = new Row
                            {
                                Time = DateTime.Parse(time),
                                RobotUUID = robotUuid,
                                Data = jsonObject
                            };

                            rows.Add(row);
                        }
                    }
                }
            }

            return rows;
        }
    }

    public class Row
    {
        public DateTime Time { get; set; }
        public string RobotUUID { get; set; }
        public dynamic Data { get; set; }
    }
}
