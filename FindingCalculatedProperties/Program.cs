using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AttributeScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            // Define the path to the folder to scan
            Console.WriteLine("Enter the path to scan:");
            string rootPath = Console.ReadLine();

            // Define the attribute you're looking for
            string attributeToLookFor = "VisibleInDetailView"; // Change to the actual attribute name you're looking for

            // Output file
            string outputPath = Path.Combine(rootPath, "AttributeScanResults.txt");

            // Start the scanning process
            ScanDirectory(rootPath, attributeToLookFor, outputPath);

            Console.WriteLine($"Scan complete. Results saved to {outputPath}");
        }

        static void ScanDirectory(string directoryPath, string attributeToLookFor, string outputPath)
        {
            // Create or overwrite the output file
            using (StreamWriter writer = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                // Get all .cs files in the directory and subdirectories
                var csFiles = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);

                foreach (var file in csFiles)
                {
                    try
                    {
                        // Process each file and write results to the output
                        var classProperties = GetPropertiesWithAttribute(file, attributeToLookFor);
                        foreach (var prop in classProperties)
                        {
                            writer.WriteLine(prop);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing file {file}: {ex.Message}");
                    }
                }
            }
        }

        static IEnumerable<string> GetPropertiesWithAttribute(string filePath, string attributeToLookFor)
        {
            List<string> result = new List<string>();
            string[] lines = File.ReadAllLines(filePath);

            // Regex patterns for detecting class, attribute, and property
            string classPattern = @"class\s+([A-Za-z_][A-Za-z0-9_]*)";
            string attributePattern = $@"{attributeToLookFor}";
            string propertyPattern = @"(public|protected|internal|private)\s+[\w\<\>\[\]]+\s+([A-Za-z_][A-Za-z0-9_]*)\s*{";

            string currentClass = null;
            List<string> attributes = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // Detect class declaration
                var classMatch = Regex.Match(line, classPattern);
                if (classMatch.Success)
                {
                    currentClass = classMatch.Groups[1].Value;
                }

                // Detect attribute
                if (line.Contains(attributePattern))
                {
                    attributes.Add(line);
                }

                // Detect property declaration
               if( line.Contains("public"))               
               {                  

                    if (attributes.Count > 0 && !string.IsNullOrEmpty(currentClass))
                    {
                        foreach (var attr in attributes)
                        {
                            result.Add($"Class: {currentClass}, Property: {line}, Attribute: {attr}");
                        }
                    }

                    // Clear attributes after the property is processed
                    attributes.Clear();
                }
            }

            return result;
        }
    }
}