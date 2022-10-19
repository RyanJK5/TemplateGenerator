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
        Console.WriteLine("0: Create Enemy");
        Console.WriteLine("1: Create Item");
        Console.WriteLine("2: Create Weapon");
        WriteFile((IDType) PromptInt("", 2), PromptString("Enter name: "));
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

    static void WriteFile(IDType type, string className) {
        string templateName;
        string fileLocation;
        switch (type) {
            case IDType.Enemy:
                templateName = "EnemyTemplate";
                fileLocation = EnemyPath;
                break;
            case IDType.Weapon:
                templateName = "WeaponTemplate";
                fileLocation = WeaponPath;
                break;
            default:
                templateName = "ItemTemplate";
                fileLocation = ItemPath;
                break;
        }
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

    static int PromptInt(string prompt, int maxNum) {
        int result;
        while (true) {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out result) && result <= maxNum && result >= 0) {
                return result;
            }
        }
    }

    static string PromptString(string prompt) {
        string? result;
        while (true) {
            Console.Write(prompt);
            result = Console.ReadLine();
            if (result != null && ValidString(result)) {
                return result;
            }
        }
    }

    static bool ValidString(string str) {
        return str.Length > 0 && char.IsLetter(str[0]) && str.All(c => char.IsLetterOrDigit(c));
    }
}