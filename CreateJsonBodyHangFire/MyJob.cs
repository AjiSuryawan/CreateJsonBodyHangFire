using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CreateJsonBodyHangFire
{
    internal class MyJob : IJob
    {

        public void mainExecute()
        {
            string ReadFolder = ConfigurationManager.AppSettings["pathFolderRead"];
            string pathFolderDestination = ConfigurationManager.AppSettings["pathFolderDestination"];
            string fileNamePrefix = ConfigurationManager.AppSettings["fileNamePrefix"];
            string latestCsvFile = GetLatestCsvFile(ReadFolder);

            if (!string.IsNullOrEmpty(latestCsvFile))
            {
                try
                {
                    // Read the CSV file using StreamReader
                    using (StreamReader reader = new StreamReader(latestCsvFile))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            // Process each line of the CSV file
                            string sessionId = Guid.NewGuid().ToString(); // Generate a unique SessionId
                            string company = "CE31"; // Replace with your actual company value

                            // Form JSON object
                            string jsonData = JsonConvert.SerializeObject(new
                            {
                                SessionId = sessionId,
                                Company = company,
                                Data = line
                            });

                            // Print or process the generated JSON string
                            Console.WriteLine(jsonData);
                            // Write the JSON data to a file in the destination folder with a unique name
                            string fileName = $"{fileNamePrefix}_{sessionId}.json";
                            string fullPath = Path.Combine(pathFolderDestination, fileName);
                            File.WriteAllText(fullPath, jsonData);
                            Console.WriteLine($"JSON data written to file: {fullPath}");

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading CSV file: " + ex.Message);
                }
            }

        }

        private string GetLatestCsvFile(string folderPath)
        {
            // Get the latest CSV file in the folder based on creation time
            try
            {
                var csvFiles = Directory.GetFiles(folderPath, "*.csv");
                if (csvFiles.Length > 0)
                {
                    return csvFiles.OrderByDescending(f => new FileInfo(f).CreationTime).First();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting latest CSV file: " + ex.Message);
            }

            return null;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            mainExecute();
        }
    }
}
