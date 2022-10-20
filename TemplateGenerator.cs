﻿namespace TemplateGenerator;

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
                WriteAdvancedFile((IDType) PromptInt("0: Create Enemy\n1: Create Item\n2: Create Weapon\n", 2), PromptString("Enter name: "));
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

    static void WriteAdvancedFile(IDType type, string className) {
        switch (type) {
            case IDType.Weapon:
                string[] lines = ReadEnumMembers("D:/repositories/bullethell/src/bullethell/combat/EnchantmentPool.java");
                for (int i = 0; i < lines.Length; i++) {
                    Console.WriteLine($"{i}: {lines[i]}");
                }
                WriteAdvancedWeapon(className, PromptString("Enter enchantment pool type: "));
                break;
        }
    }

    static void WriteAdvancedWeapon(string className, params string[] miscEntries) {
        string templateName = GetFilePathsFrom(IDType.Weapon).templateName;
        string fileLocation = GetFilePathsFrom(IDType.Weapon).fileLocation;
        
    }

    static void WriteFile(IDType type, string className) {
        string templateName = GetFilePathsFrom(type).templateName;
        string fileLocation = GetFilePathsFrom(type).fileLocation;
        string[] lines = System.IO.File.ReadAllLines("textdata/" + templateName + ".txt");
        System.IO.File.WriteAllText(fileLocation + "/" + className + ".java", "");
        using (StreamWriter writer = new(fileLocation + "/" + className + ".java", true)) {
            int hashNum = 0;
            foreach (string line in lines) {
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
                writer.WriteLine(line.Substring(line.IndexOf('#') + 1));
                hashNum++;
            }
        }
        UpdateEnum(type, className);
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

    static int PromptInt(string prompt, int maxNum) => int.Parse(PromptString(prompt, str => {
        int result;
        return int.TryParse(str, out result) && result <= maxNum && result >= 0;
    }, false));

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