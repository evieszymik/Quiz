using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;

namespace Quiz.Model
{
    public class FileHandling
    {
        public string DatabasePath { get; set; } = "C:\\QUIZY\\QUIZY2.db";

        public FileHandling()
        {
            EnsureDatabaseDirectoryExists();
            if (!File.Exists(DatabasePath))
            {
                SQLiteConnection.CreateFile(DatabasePath);
                CreateTables();
            }
            //EnsureDefaultQuestion();
        }
        private void EnsureDatabaseDirectoryExists()
        {
            string directoryPath = Path.GetDirectoryName(DatabasePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private void EnsureDefaultQuestion()
        {
            using (var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;"))
            {
                connection.Open();
                string countQuery = "SELECT COUNT(*) FROM Pytania";
                using (var command = new SQLiteCommand(countQuery, connection))
                {
                    long count = (long)command.ExecuteScalar();
                    if (count == 0)
                    {
                        AddQuestion("Jaka jest stolica Francji?", "Warszawa", "Paryż", "Bratysława", "Tokio", 'B');
                        AddQuestion("Która planeta jest najbliżej Słońca", "Ziemia", "Jowisz", "Mars", "Wenus", 'C');
                    }
                }
                connection.Close();
            }
        }

        public void AddQuestion(string questionText, string answer1, string answer2, string answer3, string answer4, char correctAnswer)
        {
            using (var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;"))
            {
                connection.Open();
                string insertQuery = "INSERT INTO Pytania (QuestionText, AnswerA, AnswerB, AnswerC,AnswerD, CorrectAnswer) VALUES (@QuestionText, @AnswerA, @AnswerB, @AnswerC, @AnswerD, @CorrectAnswer)";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@QuestionText", questionText);
                    command.Parameters.AddWithValue("@AnswerA", answer1);
                    command.Parameters.AddWithValue("@AnswerB", answer2);
                    command.Parameters.AddWithValue("@AnswerC", answer3);
                    command.Parameters.AddWithValue("@AnswerD", answer4);
                    command.Parameters.AddWithValue("@CorrectAnswer", correctAnswer);
                    command.ExecuteNonQuery();
                }
                connection.Close();

            }

        }
        private void CreateTables()
        {
            using (var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;"))
            {
                connection.Open();
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Pytania (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        QuestionText TEXT NOT NULL,
                        AnswerA TEXT NOT NULL,
                        AnswerB TEXT NOT NULL,
                        AnswerC TEXT NOT NULL,
                        AnswerD TEXT NOT NULL,
                        CorrectAnswer TEXT NOT NULL
                    )";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public List<Question> GetQuestions()
        {
            List<Question> questions = new List<Question>();
            using (var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;"))
            {
                connection.Open();
                string selectQuery = "SELECT * FROM Pytania";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string cor = reader.GetString(6);
                            bool a = false, b = false, c = false, d = false;
                            if (!string.IsNullOrEmpty(cor))
                            {
                                switch (cor)
                                {
                                    case "A":
                                        a = true;
                                        break;
                                    case "B":
                                        b = true;
                                        break;
                                    case "C":
                                        c = true;
                                        break;
                                    case "D":
                                        d = true;
                                        break;
                                }
                            }
                            
                            questions.Add(new Question
                            {
                                Id = reader.GetInt32(0),
                                QuestionText = reader.GetString(1),
                                Answers = new ObservableCollection<Answer>
                                {
                                    new Answer { Text = reader.GetString(2), Id = 'A',IsCorrect=a},
                                    new Answer { Text = reader.GetString(3), Id = 'B',IsCorrect=b },
                                    new Answer { Text = reader.GetString(4), Id = 'C' ,IsCorrect=c},
                                    new Answer { Text = reader.GetString(5), Id = 'D',IsCorrect=d }
                                },


                            });
                        }
                    }
                }
                connection.Close();
            }
            return questions;
        }
        public void SaveQuiz(Model.Quiz quiz)
        {
            string CorrectAnswer = "A";
            using (var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;"))
            {
                connection.Open();

                foreach (var question in quiz.Questions)
                {
                    string insertQuery = "INSERT INTO Pytania (QuestionText, AnswerA, AnswerB, AnswerC, AnswerD, CorrectAnswer) VALUES (@QuestionText, @AnswerA, @AnswerB, @AnswerC, @AnswerD, @CorrectAnswer)";
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@QuestionText", question.QuestionText);
                        command.Parameters.AddWithValue("@AnswerA", question.Answers[0].Text);
                        command.Parameters.AddWithValue("@AnswerB", question.Answers[1].Text);
                        command.Parameters.AddWithValue("@AnswerC", question.Answers[2].Text);
                        command.Parameters.AddWithValue("@AnswerD", question.Answers[3].Text);
                        foreach (var answer in question.Answers)
                        {
                            if (answer.IsCorrect)
                            {
                                CorrectAnswer = answer.Id.ToString();
                            }
                        }
                        command.Parameters.AddWithValue("@CorrectAnswer", CorrectAnswer);
                        command.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
        }

        //public List<Question> GetQuestions()
        //{
        //    List<Question> questions = new List<Question>();
        //    using (var connection = new SQLiteConnection($"Data Source={DatabasePath};Version=3;"))
        //    {
        //        connection.Open();
        //        string selectQuery = "SELECT * FROM Pytania";
        //        using (var command = new SQLiteCommand(selectQuery, connection))
        //        {
        //            using (var reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    questions.Add(new Question
        //                    {
        //                        Id = reader.GetInt32(0),
        //                        QuestionText = reader.GetString(1),
        //                        Answer1 = reader.GetString(2),
        //                        Answer2 = reader.GetString(3),
        //                        Answer3 = reader.GetString(4),
        //                        Answer4 = reader.GetString(5),
        //                        CorrectAnswer = reader.GetString(6)
        //                    });
        //                }
        //            }
        //        }
        //        connection.Close();
        //    }
        //    return questions;
        //}

        //private int ConvertLetterToInt(char letter)
        //{
        //    switch (letter)
        //    {
        //        case 'A':
        //            return 1;
        //        case 'B':
        //            return 2;
        //        case 'C':
        //            return 3;
        //        case 'D':
        //            return 4;
        //        default:
        //            throw new ArgumentException("Invalid answer letter: " + letter);
        //    }
        //}
        //public class Question
        //{
        //    public int Id { get; set; }
        //    public string QuestionText { get; set; }
        //    public string Answer1 { get; set; }
        //    public string Answer2 { get; set; }
        //    public string Answer3 { get; set; }
        //    public string Answer4 { get; set; }
        //    public string CorrectAnswer { get; set; }
        //}
    }
}
