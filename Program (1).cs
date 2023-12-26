using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
public class User
{
    public string Name { get; set; }
    public int CharactersPerMinute { get; set; }
    public int CharactersPerSecond { get; set; }
}

public static class Leaderboard
{
    private static List<User> users;

    public static void Load()
    {
        if (File.Exists("leaderboard.json"))
        {
            string json = File.ReadAllText("leaderboard.json");
            users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(json);
        }
        else
        {
            users = new List<User>();
        }
    }
    public static void Save()
    {
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(users, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText("leaderboard.json", json);
    }
    public static void AddUser(User user)
    {
        users.Add(user);
        Save();
    }
    public static void Print()
    {
        Console.WriteLine("Таблица рекордов:");

        foreach (var user in users.OrderBy(u => u.CharactersPerMinute))
        {
            Console.WriteLine($"Имя: {user.Name}, Символов в минуту: {user.CharactersPerMinute}, Символов в секунду: {user.CharactersPerSecond}");
        }
    }
}
public class TypingTest
{
    private string text;

    public TypingTest(string text)
    {
        this.text = text;
    }

    public void StartTest()
    {
        Console.WriteLine("Введите свое имя:");
        string name = Console.ReadLine();

        Console.WriteLine("Тест начинается. Нажмите Enter, чтобы продолжить...");
        Console.ReadLine();

        Console.Clear();

        Console.WriteLine(text);
        Console.WriteLine();
        Console.WriteLine("Набирайте текст и нажимайте Enter после каждого символа:");

        StringBuilder input = new StringBuilder();
        Stopwatch stopwatch = new Stopwatch();
        AutoResetEvent inputEvent = new AutoResetEvent(false);

        ConsoleKeyInfo keyInfo;
        bool isCompleted = false;

        Thread timerThread = new Thread(() =>
        {
            stopwatch.Start();
            while (stopwatch.Elapsed.TotalMinutes < 1 && !isCompleted)
            {
                Thread.Sleep(1000);
            }
            stopwatch.Stop();
            inputEvent.Set();
        });

        timerThread.Start();

        Thread typingThread = new Thread(() =>
        {
            do
            {
                keyInfo = Console.ReadKey(intercept: true);
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (input.Length >= text.Length)
                    {
                        isCompleted = true;
                    }
                }
                else
                {
                    if (input.Length < text.Length)
                    {
                        Console.Write(keyInfo.KeyChar);
                        input.Append(keyInfo.KeyChar);
                    }
                }
            } while (!isCompleted);
        });

        typingThread.Start();

        inputEvent.WaitOne();

        Console.WriteLine();
        Console.WriteLine("Тест завершен!");

        int charactersTyped = input.Length;
        int secondsElapsed = (int)stopwatch.Elapsed.TotalSeconds;

        int charactersPerMinute = (int)(charactersTyped / (stopwatch.Elapsed.TotalMinutes));
        int charactersPerSecond = (int)(charactersTyped / secondsElapsed);

        User user = new User
        {
            Name = name,
            CharactersPerMinute = charactersPerMinute,
            CharactersPerSecond = charactersPerSecond
        };

        Leaderboard.AddUser(user);

        Leaderboard.Print();
    }
}

class Program
{
    static void Main()
    {
        Leaderboard.Load();

        string testText = "Практическая 8 - Тест на скоропечатание 21 нояб.Срок сдачи: 5 дек., 23:59 Необходимо в консоли реализовать тест на скоропечатание с таблицей рекордов.";
        TypingTest typingTest = new TypingTest(testText);
        typingTest.StartTest();
    }
}