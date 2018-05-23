using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MHWJewelMelderInfo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Version v = Assembly.GetEntryAssembly().GetName().Version;
            Console.WriteLine($"v{v.Major}.{v.Minor}.{v.Build}");
            Console.WriteLine();

            if (args.Contains("-h") || args.Contains("--help"))
            {
                Console.WriteLine("--no-jewel-suffix   Does not print the ' Jewel X' decoration name suffix.");
                Console.WriteLine("--add-skill-name    Appends the name of the skill after the decoration name.");
                Console.WriteLine("--no-custom-order   Does not sort decorations using the ordering file.");
            }

            bool noJewelSuffix = args.Contains("--no-jewel-suffix");
            bool addSkillName = args.Contains("--add-skill-name");
            bool gameOrder = args.Contains("--no-custom-order") == false;

            var httpClient = new HttpClient();

            string decorationsContent = await httpClient.GetStringAsync("https://mhw-db.com/decorations?p={%22id%22:true,%22name%22:true,%20%22skills.skill%22:%20true}");
            string skillsContent = await httpClient.GetStringAsync("https://mhw-db.com/skills?p={%22id%22:true,%22name%22:true,%22ranks.id%22:true}");

            IEnumerable<Decoration> decorations = JsonConvert.DeserializeObject<IList<Decoration>>(decorationsContent);
            Skill[] skills = JsonConvert.DeserializeObject<Skill[]>(skillsContent);

            if (gameOrder)
            {
                string orderingFilename = Path.Combine(AppContext.BaseDirectory, "data", "decorations_ordering.json");
                if (File.Exists(orderingFilename))
                {
                    Dictionary<int, Decoration> decorationsMap = decorations.ToDictionary(x => x.Id);
                    Identifier[] orderingObjects = JsonConvert.DeserializeObject<Identifier[]>(File.ReadAllText(orderingFilename));

                    for (int i = 0; i < orderingObjects.Length; i++)
                    {
                        if (decorationsMap.TryGetValue(orderingObjects[i].Id, out Decoration deco))
                            deco.Ordering = i;
                    }

                    decorations = decorations.OrderBy(x => x.Ordering);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Could not find decoration ordering file, skip ordering.");
                    Console.ResetColor();
                }
            }

            var output = Console.Out;

            foreach (Decoration decoration in decorations)
            {
                string name = decoration.Name;
                if (noJewelSuffix)
                {
                    int index = decoration.Name.IndexOf(" Jewel ");
                    name = name.Substring(0, index);
                }

                Skill associatedSkill = skills.FirstOrDefault(x => x.Id == decoration.Skills[0].Id);
                if (associatedSkill.Id == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Could not find skill for decoration '{decoration.Name}' (skill id {decoration.Skills[0].Id})");
                    Console.ResetColor();
                }
                else
                {
                    if (addSkillName)
                        name += $" ({associatedSkill.Name})";

                    output.WriteLine($"{name}: {associatedSkill.Ranks.Length}");
                }
            }
        }
    }

    public class Identifier
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }

    public class SkillIdentifier
    {
        [JsonProperty("skill")]
        public int Id { get; set; }
    }

    public class Skill
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("ranks")]
        public Identifier[] Ranks { get; set; }
    }

    public class Decoration
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("skills")]
        public SkillIdentifier[] Skills { get; set; }

        public int Ordering;
    }
}
