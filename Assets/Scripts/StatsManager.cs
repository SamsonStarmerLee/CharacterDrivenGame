using Assets.Scripts.Actions;
using Assets.Scripts.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts
{
    // TODO: C#8 usings
    public sealed class StatsManager : MonoBehaviour
    {
        private string savePath;
        private string wordsPath;

        private static HashSet<string> wordHistory = new HashSet<string>();

        private void Awake()
        {
            savePath = Path.Combine(Application.persistentDataPath, "SaveData");
            wordsPath = Path.Combine(Application.persistentDataPath, "MatchedWords");

            Load();
        }

        private void OnEnable()
        {
            this.AddObserver(OnScoreWord, Notify.Action<ScoreAction>());
            this.AddObserver(OnGameEnd, GameManager.GameOverNotification);
        }

        private void OnDisable()
        {
            this.RemoveObserver(OnScoreWord, Notify.Action<ScoreAction>());
            this.RemoveObserver(OnGameEnd, GameManager.GameOverNotification);
        }

        private void Load()
        {
            if (!File.Exists(savePath))  SaveScore();
            if (!File.Exists(wordsPath)) SaveWords();

            using (var reader = new StreamReader(File.Open(savePath, FileMode.Open)))
            {
                var totalScore = reader.ReadLine();
                var totalFloors = reader.ReadLine();

                // TODO: Do something with this.
            }

            using (var reader = new StreamReader(File.Open(wordsPath, FileMode.Open)))
            {
                wordHistory.Clear();

                string word;
                while ((word = reader.ReadLine()) != null)
                {
                    wordHistory.Add(word);
                }
            }
        }

        private void OnScoreWord(object sender, object args)
        {
            var action = args as ScoreAction;
            
            if (action.Word != null && !wordHistory.Contains(action.Word))
            {
                wordHistory.Add(action.Word);
            }
        }

        private void OnGameEnd(object sender, object args)
        {
            SaveScore();
            SaveWords();
        }

        private void OnApplicationQuit()
        {
            SaveScore();
            SaveWords();
        }

        private void SaveScore()
        {
            // TEMP
            var gm = FindObjectOfType<GameManager>();
            var gameScore  = gm.Score;
            var gameFloors = gm.Floor;

            using (var reader = new StreamReader(File.Open(savePath, FileMode.Open)))
            {
                gameScore  += int.Parse(reader.ReadLine());
                gameFloors += int.Parse(reader.ReadLine()) - 1;
            }

            using (var writer = new StreamWriter(File.Open(savePath, FileMode.Create)))
            {
                writer.WriteLine(gameScore);
                writer.WriteLine(gameFloors);
            }
        }

        private void SaveWords()
        {
            using (var writer = new StreamWriter(File.Open(wordsPath, FileMode.Create)))
            {
                foreach (var w in wordHistory)
                {
                    writer.Write(w + Environment.NewLine);
                }
            }
        }
    }
}
