using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Rienet
{
    public static class FileSectionReader
    {
        /// <summary>
        /// most symbols require english except chat ones, commands with <> require chinese (simplified), for specifics check list of preset commands and remember to make a list of your custom commands
        /// </summary>
        /// <param name="file"> path to the cutscene file and its name </param>
        public static Dictionary<string, List<string>> ReadSections(string file)
        {
            string[] lines = File.ReadAllLines(file);

            //make map of sections, each section is an array of lines with their speaker (don't map these yet, just split function when using them, this is to prevent mapping duplicates)
            Dictionary<string, List<string>> sections = new();

            List<string> section = null;

            for (int i = 0; i < lines.Length; i++)
            {
                //search for identifiers, save these sections, the order doesn't really matter, as long as one section is top-down
                string line = lines[i];

                int start = line.IndexOf("<");
                if (start >= 0)
                {
                    string identifier = line.Substring(start + 1);
                    if (identifier.StartsWith("/"))
                    {
                        //mark end of section
                        sections.Add(identifier.Substring(1, identifier.Length - 2), section);
                        //Debug.WriteLine(identifier.Substring(1, identifier.Length - 2));
                        section = null;
                    }
                    else
                    {
                        //mark beginning identifier check
                        section = new();
                    }
                }
                //ignore comments and empty lines or out of bounds areas
                else if (!(line.Equals("") || section == null || line.StartsWith("//")))
                {
                    //write to current section (dialogue, commands, etc)
                    section.Add(line);
                }
            }

            return sections;
        }
    }
}