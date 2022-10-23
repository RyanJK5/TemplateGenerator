namespace TemplateGenerator;

static class TemplateGenerator {

    const string EnemyPath = "D:/repositories/bullethell/src/bullethell/enemies"; 
    const string ItemPath = "D:/repositories/bullethell/src/bullethell/items";
    const string WeaponPath = "D:/repositories/bullethell/src/bullethell/items/weapons";

    const string EnemyEnumPath = "D:/repositories/bullethell/src/bullethell/enemies/EnemyID.java";
    const string ItemEnumPath = "D:/repositories/bullethell/src/bullethell/items/ItemID.java";

    enum IDType {
        Enemy, Item, Weapon
    }

    static void Main() {
        if (char.ToUpper(PromptString("Use advanced mode? (Y/N): ", 
            str => str.Length > 0 && (char.ToUpper(str[0]) == 'Y' || char.ToUpper(str[0]) == 'N'), true)[0]) == 'Y') {
                AdvancedFilePrompts((IDType) PromptInt("0: Create Enemy\n1: Create Item\n2: Create Weapon\n", 2), PromptString("Enter name: "));
                return;
        } else {
            WriteFile((IDType) PromptInt("0: Create Enemy\n1: Create Item\n2: Create Weapon\n", 2), PromptString("Enter name: "));
        }
    }

    static string CreateEnumName(string className) {
        string result = "";
        bool first = true;
        foreach (char c in className) {
            if (!first && char.IsUpper(c)) {
                result += "_";
            }
            first = false;
            result += char.ToUpper(c);
        }
        return result;
    }

    static string CreateStringName(string className) {
        string result = "";
        bool first = true;
        foreach (char c in className) {
            if (!first && char.IsUpper(c)) {
                result += " ";
            }
            first = false;
            result += c;
        }
        return result;
    }

    static void AdvancedFilePrompts(IDType type, string className) {
        switch (type) {
            case IDType.Weapon:
                Predicate<string> checkForValidArg = str => str.Length > 0 && (int.TryParse(str, out int result) || str[0] == '\\');
                Console.WriteLine("Enter \\ to omit any value");
                WriteFile(type, className,
                    new string[] { PromptString("Enter description: ", str => str.Length > 0, true) },
                    new string[] { PromptString("Enter crit multiplier: ", str => 
                        str.Length > 0 && (float.TryParse(str, out float result) || str[0] == '\\'), true) },
                    new string[] { PromptString("Enter damage: ", checkForValidArg, true) },
                    new string[] { PromptString("Enter mana cost: ", checkForValidArg, true) },
                    new string[] { PromptString("Enter fire time: ", checkForValidArg, true) },
                    new string[] { PromptString("Enter range: ", checkForValidArg, true) },
                    PromptFromEnum("D:/repositories/bullethell/src/bullethell/combat/EnchantmentPool.java", "Enter enchantment pool type: ", false),
                    PromptFromEnum("D:/repositories/bullethell/src/bullethell/combat/tags/StatusEffectType.java", 
                        "Enter allowed status effects (\\ to break)", true)
                );
                break;
        }
    }

    static void WriteFile(IDType type, string className, params string[][] advEntries) {
        string[] lines = System.IO.File.ReadAllLines("textdata/" + GetFilePathsFrom(type).templateName + ".txt");
        using (StreamWriter writer = new(GetFilePathsFrom(type).fileLocation + "/" + className + ".java")) {
            int hashNum = 0;
            int advIndex = 0;
            bool advMode = advEntries.Length > 0;
            foreach (string line in lines) {
                if (advMode && line.Contains('$') && advIndex < advEntries.Length) {
                    if (advEntries[advIndex][0][0] == '\\') {
                        advIndex++;
                        continue;
                    }
                    if (line.Contains('|')) {
                        writer.Write(line.Substring(0, line.IndexOf('|')));
                        string pipedSection = line.Substring(line.IndexOf('|'), line.LastIndexOf('|') - line.IndexOf('|'));
                        foreach (string entry in advEntries[advIndex]) {
                            writer.Write(pipedSection.Substring(1, pipedSection.IndexOf('$') - 1) + entry + 
                                pipedSection.Substring(pipedSection.IndexOf('$') + 1, pipedSection.Length - 1 - pipedSection.IndexOf('$')));
                        }
                        writer.WriteLine(line.Substring(line.LastIndexOf('|') + 1));
                        advIndex++;
                        continue;
                    }
                    writer.WriteLine(line.Substring(0, line.IndexOf('$')) + advEntries[advIndex][0] + line.Substring(line.IndexOf('$') + 1));
                    advIndex++;
                    continue;
                }
                if (!line.Contains('#') || hashNum > 2) {
                    writer.WriteLine(line);
                    continue;
                }
                writer.Write(line.Substring(0, line.IndexOf('#')));
                switch (hashNum) {
                    case 0:
                        writer.Write(className);
                        break;
                    case 1:
                        writer.Write(CreateEnumName(className));
                        break;
                    case 2:
                        writer.Write(CreateStringName(className));
                        break;
                }
                hashNum++;
                writer.WriteLine(line.Substring(line.IndexOf('#') + 1));
            }
        }
        UpdateEnum(type, className);
    }

    static string[] PromptFromEnum(string filePath, string prompt, bool loop) {
        string[] lines = ReadEnumMembers(filePath);
        for (int i = 0; i < lines.Length; i++) {
            Console.WriteLine($"{i}: {lines[i]}");
        }
        if (!loop) {
            return new string[] { lines[PromptInt(prompt, lines.Length - 1)] };
        }
        List<string> entries = new();
        while (true) {
            string str = PromptString("Enter allowed status effects (\\ to break): ", 
                s => s.Length > 0 && ((int.TryParse(s, out int result) && result < lines.Length && result >= 0) || s[0] == '\\'), 
                true);
            if (str[0] == '\\') {
                break;
            }
            entries.Add(lines[int.Parse(str)]);
        }
        return entries.ToArray();
    }

    static string[] ReadEnumMembers(string filePath) {
        string[] lines = System.IO.File.ReadAllLines(filePath);
        List<string> result = new();
        char[] chars = {'(', ','};
        for (int i = Array.FindIndex(lines, str => str.Contains("enum")) + 1; i < lines.Length; i++) {
            foreach (char c in chars) {
                if (lines[i].Contains(c)) {
                    int startIndex = Array.FindIndex(lines[i].ToCharArray(), c => char.IsLetter(c));
                    result.Add(lines[i].Substring(startIndex, lines[i].IndexOf(c) - startIndex));
                    if (lines[i].Contains(';')) {
                        return result.ToArray();
                    }
                    break;
                }
            }
        }
        return result.ToArray();
    }

    static (string templateName, string fileLocation) GetFilePathsFrom(IDType type) {
        switch (type) {
            case IDType.Enemy:
                return ("EnemyTemplate", EnemyPath);
            case IDType.Weapon:
                return ("WeaponTemplate", WeaponPath);
            default:
                return ("ItemTemplate", ItemPath);
        }
    }
    static void UpdateEnum(IDType type, string className) {
        string filePath;
        switch (type) {
            case IDType.Enemy:
                filePath = EnemyEnumPath;
                break;
            default:
                filePath = ItemEnumPath;
                break;
        }

        string[] lines = System.IO.File.ReadAllLines(filePath);
        using (StreamWriter writer = new(filePath, false)) {
            bool inRangeToPlaceObject = false;
            foreach (string line in lines) {
                if (!inRangeToPlaceObject && line.Contains("enum")) {
                    inRangeToPlaceObject = true;
                }
                if (inRangeToPlaceObject && line.Contains(';')) {
                    writer.WriteLine(line.Substring(0, line.Length - 1) + ",");
                    writer.WriteLine("\t" + CreateEnumName(className) + "(" + className + ".class);");
                    inRangeToPlaceObject = false;
                    continue;
                }
                writer.WriteLine(line);
            }
        }
    }

    static int PromptInt(string prompt, int maxNum) => int.Parse(PromptString(prompt, str => 
        int.TryParse(str, out int result) && result <= maxNum && result >= 0, false));

    static int PromptInt(string prompt, bool repeat) => int.Parse(PromptString(prompt, str =>
        int.TryParse(str, out int result), repeat));

    static float PromptFloat(string prompt, float maxNum) => float.Parse(PromptString(prompt, str =>
        float.TryParse(str, out float result) && result <= maxNum && result >= 0, false));

    static string PromptString(string prompt, Predicate<string> predicate, bool repeatPrompt) {
        string? result;
        bool first = true;
        while (true) {
            if (repeatPrompt || first) {
                Console.Write(prompt);
            }
            first = false;
            result = Console.ReadLine();
            if (result != null && predicate.Invoke(result)) {
                return result;
            }
        }
    }

    static string PromptString(string prompt, bool repeatPrompt) => PromptString(prompt, 
        str => str.Length > 0 && char.IsLetter(str[0]) && str.All(c => char.IsLetterOrDigit(c)), repeatPrompt);

    static string PromptString(string prompt) => PromptString(prompt, true);

}