using System.Diagnostics;

namespace MJU23v_DTP_T2
{
    internal class Program
    {
        static List<Link> links = new List<Link>();

        class Link
        {
            public string Category { get; set; }
            public string Group { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }

            public Link(string category, string group, string name, string description, string url)
            {
                Category = category;
                Group = group;
                Name = name;
                Description = description;
                Url = url;
            }

            public Link(string line)
            {
                // FIXME: Kontrollera att raden innehåller exakt 5 delar innan split.
                string[] parts = line.Split('|');
                if (parts.Length != 5)
                {
                    throw new ArgumentException("Felaktigt format i länkraden.");
                }
                Category = parts[0];
                Group = parts[1];
                Name = parts[2];
                Description = parts[3];
                Url = parts[4];
            }

            public void Print(int index)
            {
                Console.WriteLine($"|{index,-2}|{Category,-10}|{Group,-10}|{Name,-20}|{Description,-40}|");
            }

            public void OpenLink()
            {
                // FIXME: Lägg till felhantering om länken är ogiltig eller inte kan öppnas.
                Process application = new Process();
                application.StartInfo.UseShellExecute = true;
                application.StartInfo.FileName = Url;
                application.Start();
            }

            public string Serialize()
            {
                return $"{Category}|{Group}|{Name}|{Description}|{Url}";
            }
        }

        static void Main(string[] args)
        {
            string filePath = @"..\..\..\links\links.lis";

            try
            {
                // FIXME: Kontrollera om filen existerar innan inläsning.
                links = LoadLinksFromFile(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid inläsning av fil: {ex.Message}");
            }

            Console.WriteLine("Välkommen till länklistan! Skriv 'hjälp' för hjälp!");

            do
            {
                Console.Write("> ");
                string cmdInput = Console.ReadLine().Trim();

                if (string.IsNullOrWhiteSpace(cmdInput))
                {
                    HandleUnknownCommand("");
                    continue;
                }

                string[] cmdParts = cmdInput.Split();
                string command = cmdParts[0];

                if (command == "sluta")
                {
                    Console.WriteLine("Hej då! Välkommen åter!");
                    break;
                }
                else if (command == "hjälp")
                {
                    PrintHelp();
                }
                else if (command == "lista")
                {
                    if (links.Count == 0)
                    {
                        Console.WriteLine("Inga länkar att visa.");
                    }
                    else
                    {
                        int index = 0;
                        foreach (Link linkObj in links)
                            linkObj.Print(index++);
                    }
                }
                else if (command == "ny")
                {
                    Console.WriteLine("Skapa en ny länk:");
                    Console.Write("  ange kategori: ");
                    string category = Console.ReadLine();
                    Console.Write("  ange grupp: ");
                    string group = Console.ReadLine();
                    Console.Write("  ange namn: ");
                    string name = Console.ReadLine();
                    Console.Write("  ange beskrivning: ");
                    string description = Console.ReadLine();
                    Console.Write("  ange länk: ");
                    string url = Console.ReadLine();

                    // FIXME: Validera URL-formatet innan du lägger till länken.
                    Link newLink = new Link(category, group, name, description, url);
                    links.Add(newLink);
                }
                else if (command == "spara")
                {
                    if (cmdParts.Length == 2)
                    {
                        filePath = $@"..\..\..\links\{cmdParts[1]}";
                        try
                        {
                            using (StreamWriter writer = new StreamWriter(filePath))
                            {
                                foreach (Link linkObj in links)
                                {
                                    writer.WriteLine(linkObj.Serialize());
                                }
                            }
                            Console.WriteLine($"Länkar sparade till {filePath}.");
                        }
                        catch (Exception ex)
                        {
                            // FIXME: Lägg till bättre felmeddelande för skrivfel.
                            Console.WriteLine($"Fel vid sparning av fil: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Fel: Ange ett filnamn för att spara.");
                    }
                }
                else if (command == "ladda")
                {
                    if (cmdParts.Length == 2)
                    {
                        filePath = $@"..\..\..\links\{cmdParts[1]}";
                        try
                        {
                            links = LoadLinksFromFile(filePath);
                            Console.WriteLine($"Länkar laddade från {filePath}.");
                        }
                        catch (Exception ex)
                        {
                            // FIXME: Lägg till bättre felmeddelande för ogiltig filväg.
                            Console.WriteLine($"Fel vid laddning av fil: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Fel: Ange en giltig filväg.");
                    }
                }
                else if (command == "ta")
                {
                    if (cmdParts.Length >= 3 && cmdParts[1] == "bort" && int.TryParse(cmdParts[2], out int index))
                    {
                        if (index >= 0 && index < links.Count)
                        {
                            links.RemoveAt(index);
                            Console.WriteLine($"Länk på index {index} har tagits bort.");
                        }
                        else
                        {
                            // FIXME: Förbättra felmeddelandet för ogiltigt index.
                            Console.WriteLine($"Fel: Index {index} är ogiltigt. Ange ett värde mellan 0 och {links.Count - 1}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Fel: Ogiltigt kommando eller argument för 'ta bort'.");
                    }
                }
                else if (command == "öppna")
                {
                    if (cmdParts.Length >= 3 && cmdParts[1] == "grupp")
                    {
                        string groupName = cmdParts[2];
                        var groupLinks = links.Where(link => link.Group == groupName).ToList();

                        if (groupLinks.Any())
                        {
                            foreach (Link linkObj in groupLinks)
                            {
                                linkObj.OpenLink();
                            }
                        }
                        else
                        {
                            // FIXME: Informera användaren om att gruppen inte hittades.
                            Console.WriteLine($"Fel: Gruppen '{groupName}' hittades inte.");
                        }
                    }
                    else if (cmdParts.Length >= 3 && cmdParts[1] == "länk" && int.TryParse(cmdParts[2], out int linkIndex))
                    {
                        if (linkIndex >= 0 && linkIndex < links.Count)
                        {
                            links[linkIndex].OpenLink();
                        }
                        else
                        {
                            // FIXME: Lägg till bättre felmeddelande för ogiltigt länkindex.
                            Console.WriteLine($"Fel: Index {linkIndex} är ogiltigt. Ange ett värde mellan 0 och {links.Count - 1}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Fel: Ogiltigt kommando eller argument för 'öppna'.");
                    }
                }
                else
                {
                    HandleUnknownCommand(command);
                }

            } while (true);
        }

        private static List<Link> LoadLinksFromFile(string filePath)
        {
            List<Link> linksList = new List<Link>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                int index = 0;
                string currentLine;

                while ((currentLine = reader.ReadLine()) != null)
                {
                    Link linkObj = new Link(currentLine);
                    linksList.Add(linkObj);
                }
            }

            return linksList;
        }

        private static void PrintHelp()
        {
            // Fullständig hjälptext med detaljer och exempel.
            Console.WriteLine("Kommandon och deras användning:");

            // Hjälp för generella kommandon.
            Console.WriteLine("hjälp           - Skriv ut denna hjälptext.");
            Console.WriteLine("sluta           - Avsluta programmet.");

            // Hjälp för kommandon som hanterar länkar.
            Console.WriteLine("lista           - Lista alla länkar i systemet.");

            // Hjälp för att skapa nya länkar.
            Console.WriteLine("ny              - Skapa en ny länk. Du kommer att bli ombedd att ange:");
            Console.WriteLine("                  kategori, grupp, namn, beskrivning och URL.");

            // Hjälp för att spara och ladda filer.
            Console.WriteLine("spara <filnamn> - Spara alla länkar till en fil med angivet namn. Exempel:");
            Console.WriteLine("                  spara mina_lankar.txt");

            // Hjälp för att ladda filer.
            Console.WriteLine("ladda <filnamn> - Ladda länkar från en fil med angivet namn. Exempel:");
            Console.WriteLine("                  ladda mina_lankar.txt");

            // Hjälp för att ta bort länkar.
            Console.WriteLine("ta bort <index> - Ta bort länken på det angivna indexet. Exempel:");
            Console.WriteLine("                  ta bort 0");

            // Hjälp för att öppna länkar.
            Console.WriteLine("öppna länk <index> - Öppna en specifik länk baserat på dess index. Exempel:");
            Console.WriteLine("                     öppna länk 0");

            // Hjälp för att öppna grupper av länkar.
            Console.WriteLine("öppna grupp <gruppnamn> - Öppna alla länkar i den angivna gruppen. Exempel:");
            Console.WriteLine("                          öppna grupp Skola");
        }

        private static void HandleUnknownCommand(string command)
        {
            // FIXME: Gör felmeddelandet mer användarvänligt.
            Console.WriteLine($"Okänt kommando: '{command}'");
        }
    }
}
