using Quiz.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Controls;
using Microsoft.Win32;


namespace Quiz.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private FileHandling fileHandling;
        private DispatcherTimer dispatcherTimer;
        private Stopwatch stopwatch;
        private bool isRunning;
        private int currentQuestionIndex;
        private int points;

        public event PropertyChangedEventHandler PropertyChanged;
        private Model.Quiz _currentQuiz;
        private ObservableCollection<Question> _questions;
        private Question _selectedQuestion;
        private string _quizName;
        private string _newQuestionText;
        private string _answerA;
        private string _answerB;
        private string _answerC;
        private string _answerD;
        private string _correct;

        public ICommand StartCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand TextBoxDoubleClickCommand { get; }

        public ICommand AddQuestionCommand { get; }
        public ICommand DeleteQuestionCommand { get; }
        public ICommand SaveQuizCommand { get; }
        public ICommand ReadQuizCommand { get; }

        //public ObservableCollection<Question> Questions { get; set; }
        public ObservableCollection<Question> Questions
        {
            get => _questions;
            set
            {
                _questions = value;
                OnPropertyChanged(nameof(Questions));
            }
        }

        public string QuizName
        {
            get => _quizName;
            set
            {
                _quizName = value;
                OnPropertyChanged(nameof(QuizName));

            }
        }
        public string NewQuestionText
        {
            get => _newQuestionText;
            set
            {
                if (_selectedQuestion != null)
                {
                    _selectedQuestion.QuestionText = value;
                }
                _newQuestionText = value;
                OnPropertyChanged(nameof(NewQuestionText));

            }
        }
        public string AnswerA
        {
            get => _answerA;
            set
            {
                if (_selectedQuestion != null)
                {
                    _selectedQuestion.Answers[0].Text = value;
                }
                _answerA = value;
                OnPropertyChanged(nameof(AnswerA));

            }
        }
        public string AnswerB
        {
            get => _answerB;
            set
            {
                if (_selectedQuestion != null)
                {
                    _selectedQuestion.Answers[1].Text = value;
                }
                _answerB = value;
                OnPropertyChanged(nameof(AnswerB));
            }
        }
        public string AnswerC
        {
            get => _answerC;
            set
            {
                if (_selectedQuestion != null)
                {
                    _selectedQuestion.Answers[2].Text = value;
                }
                _answerC = value;
                OnPropertyChanged(nameof(AnswerC));

            }
        }
        public string AnswerD
        {
            get => _answerD;
            set
            {
                if (_selectedQuestion != null)
                {
                    _selectedQuestion.Answers[3].Text = value;
                }
                _answerD = value;
                OnPropertyChanged(nameof(AnswerD));
            }
        }
        public string Correct
        {
            get => _correct;
            set
            {
                _correct = value;
                if (_selectedQuestion != null)
                {

                    foreach (var answer in _selectedQuestion.Answers)
                    {
                        answer.IsCorrect = answer.Id.ToString() == _correct;
                    }
                }
                OnPropertyChanged(nameof(Correct));
            }
        }
        public Question SelectedQuestion
        {
            get { return _selectedQuestion; }
            set
            {
                _selectedQuestion = value;
                OnPropertyChanged(nameof(SelectedQuestion));
                if (_selectedQuestion != null)
                {
                    NewQuestionText = _selectedQuestion.QuestionText;
                    AnswerA = _selectedQuestion.Answers.Count > 0 ? _selectedQuestion.Answers[0]?.Text : string.Empty;
                    AnswerB = _selectedQuestion.Answers.Count > 1 ? _selectedQuestion.Answers[1]?.Text : string.Empty;
                    AnswerC = _selectedQuestion.Answers.Count > 2 ? _selectedQuestion.Answers[2]?.Text : string.Empty;
                    AnswerD = _selectedQuestion.Answers.Count > 3 ? _selectedQuestion.Answers[3]?.Text : string.Empty;

                    var correctAnswer = _selectedQuestion.Answers.FirstOrDefault(a => a.IsCorrect);
                    Correct = correctAnswer != null ? correctAnswer.Id.ToString() : string.Empty;
                }
                else
                {
                    NewQuestionText = AnswerA = AnswerB = AnswerC = AnswerD = Correct = string.Empty;
                }

            }
        }

        public Question CurrentQuestion { get; set; }
        public string TimerText { get; set; }
        public int Points
        {
            get { return points; }
            set
            {
                points = value;
                OnPropertyChanged(nameof(Points));
            }
        }
        private string quizDuration;
        public string QuizDuration
        {
            get { return quizDuration; }
            set
            {
                quizDuration = value;
                OnPropertyChanged(nameof(QuizDuration));
            }
        }

        public Model.Quiz CurrentQuiz
        {
            get => _currentQuiz;
            set
            {
                _currentQuiz = value;
                OnPropertyChanged(nameof(CurrentQuiz));
            }
        }


        public MainViewModel()
        {
            fileHandling = new FileHandling();
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            stopwatch = new Stopwatch();
            Questions = new ObservableCollection<Question>();

            StartCommand = new RelayCommand(StartQuizAndOpenWindow);
            CreateCommand = new RelayCommand(CreateQuizAndOpenWindow);
            StopCommand = new RelayCommand(StopQuiz);
            LoadCommand = new RelayCommand(LoadQuestionsFromSelectedFile);
            TextBoxDoubleClickCommand = new RelayCommand<TextBox>(TextBoxDoubleClick);

            TimerText = "00:00.000";


            AddQuestionCommand = new RelayCommand(AddQuestion);
            DeleteQuestionCommand = new RelayCommand(DeleteQuestion);
            SaveQuizCommand = new RelayCommand(SaveQuiz);
            ReadQuizCommand = new RelayCommand(ReadQuiz);
            _currentQuiz = new Model.Quiz { Name = QuizName, Questions = new ObservableCollection<Question>(Questions) };
        }
        private void AddQuestion()
        {

            if (!string.IsNullOrWhiteSpace(NewQuestionText) && !string.IsNullOrWhiteSpace(AnswerA) && !string.IsNullOrWhiteSpace(AnswerB) && !string.IsNullOrWhiteSpace(AnswerC) && !string.IsNullOrWhiteSpace(AnswerD) && !string.IsNullOrWhiteSpace(Correct))
            {
                bool a = false, b = false, c = false, d = false;
                switch (Correct)
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
                var newAnswerA = new Answer { Id = 'A', Text = AnswerA, IsCorrect = a };
                var newAnswerB = new Answer { Id = 'B', Text = AnswerB, IsCorrect = b };
                var newAnswerC = new Answer { Id = 'C', Text = AnswerC, IsCorrect = c };
                var newAnswerD = new Answer { Id = 'D', Text = AnswerD, IsCorrect = d };
                var newQuestion = new Question { Id = Questions.Count + 1, QuestionText = NewQuestionText, Answers = { newAnswerA, newAnswerB, newAnswerC, newAnswerD } };
                Questions.Add(newQuestion);
                _currentQuiz.Questions.Add(newQuestion);
                OnPropertyChanged(nameof(Questions));
                OnPropertyChanged(nameof(CurrentQuiz));
                //_currentQuiz.Questions.Add(new Model.Question());
                //onPropertyChanged(nameof(_currentQuiz));
                NewQuestionText = string.Empty;
                AnswerA = string.Empty;
                AnswerB = string.Empty;
                AnswerC = string.Empty;
                AnswerD = string.Empty;
            }
            else
            {
                MessageBox.Show("Pole jest puste!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

        }

        private void DeleteQuestion()
        {
            if (SelectedQuestion != null)
            {
                Questions.Remove(SelectedQuestion);
                _currentQuiz.Questions.Remove(SelectedQuestion);
                SelectedQuestion = null;
                OnPropertyChanged(nameof(Questions));
                OnPropertyChanged(nameof(CurrentQuiz));
            }
            else
            {
                MessageBox.Show("Nie wybrano żadnego pytania do usunięcia.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveQuiz()
        {
            fileHandling.SaveQuiz(_currentQuiz);
            MessageBox.Show("Pomyslnie zapisano do bazy danych", "Komunikat");
        }

        private void ReadQuiz()
        {
            var questions = fileHandling.GetQuestions();
            Questions.Clear();
            foreach (var question in questions)
            {
                Questions.Add(question);
                OnPropertyChanged(nameof(Questions));
            }

        }

        private void StartQuizAndOpenWindow()
        {
           
           
            if (isRunning != true)
            {

                OpenSolveWindow();
                isRunning = true;
            }
        }
        private void CreateQuizAndOpenWindow()
        {
            if (isRunning != true)
            {

                OpenCreateWindow();
                isRunning = true;
            }
        }
        private void LoadQuestionsFromSelectedFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Database Files (*.db)|*.db|All Files (*.*)|*.*";
            openFileDialog.InitialDirectory = "C:\\QUIZY";

            if (openFileDialog.ShowDialog() == true)
            {
                fileHandling.DatabasePath = openFileDialog.FileName;
                LoadQuestionsFromDatabase();
            }
            stopwatch.Start();
            dispatcherTimer.Start();

        }

        private void StopQuiz()
        {
            stopwatch.Stop();
            dispatcherTimer.Stop();
            isRunning = false;
            TimeSpan quizDuration = stopwatch.Elapsed;
            QuizDuration = quizDuration.ToString(@"mm\:ss\.fff");

            currentQuestionIndex = 0;       
            MessageBox.Show($"Koniec testu. Twój wynik: {Points}/{Questions.Count}, Twój czas: {QuizDuration} ", "Koniec testu", MessageBoxButton.OK, MessageBoxImage.Information);
            Points = 0;
            stopwatch.Reset();

        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            TimerText = stopwatch.Elapsed.ToString(@"mm\:ss\.fff");
            OnPropertyChanged(nameof(TimerText));
        }

        private void LoadQuestions()
        {
            var questions = fileHandling.GetQuestions();
            Questions.Clear();
            foreach (var question in questions)
            {
                Questions.Add(question);
                OnPropertyChanged(nameof(Questions));
            }
            currentQuestionIndex = 0;
        }

        private void DisplayQuestion()
        {
            if (Questions.Count == 0)
            {
                MessageBox.Show("No questions available.");
                return;
            }

            CurrentQuestion = Questions[currentQuestionIndex];
            OnPropertyChanged(nameof(CurrentQuestion));
        }

        public void TextBoxDoubleClick(object sender)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
                CheckAnswer(textBox);
                currentQuestionIndex++;
                if (currentQuestionIndex >= Questions.Count)
                {
                    
                    stopwatch.Stop();
                    dispatcherTimer.Stop();
                    isRunning = false;
                    TimeSpan quizDuration = stopwatch.Elapsed;
                    QuizDuration = quizDuration.ToString(@"mm\:ss\.fff");

                    currentQuestionIndex = 0;
                   
                    MessageBox.Show($"Koniec testu. Twój wynik: {Points}/{Questions.Count}, Twój czas: {QuizDuration} ", "Koniec testu", MessageBoxButton.OK, MessageBoxImage.Information);
                    Points = 0;
                    stopwatch.Reset();

                }
                else
                {
                    DisplayQuestion();
                }
            }
        }

        private void CheckAnswer(TextBox textBox)
        {
            var currentQuestion = Questions[currentQuestionIndex];
            string correctAnswer="A"; /*currentQuestion.CorrectAnswer*/
            foreach(var answer in currentQuestion.Answers)
            {
                if(answer.IsCorrect)
                {
                    correctAnswer = answer.Id.ToString();
                }
            }
            Debug.WriteLine($"Checking answer. TextBox: {textBox.Name}, CorrectAnswer: {correctAnswer}");

            if ((textBox.Name == "TextBox1" && correctAnswer == "A") ||
                   (textBox.Name == "TextBox2" && correctAnswer == "B") ||
                   (textBox.Name == "TextBox3" && correctAnswer == "C") ||
                   (textBox.Name == "TextBox4" && correctAnswer == "D"))
            {
                Points++;

            }



        }

        private void LoadQuestionsFromDatabase()
        {
            LoadQuestions();
            DisplayQuestion();

        }

        private void OpenSolveWindow()
        {
            Window_Solve solveWindow = new Window_Solve();
            solveWindow.Show();
            
            
        }
        private void OpenCreateWindow()
        {
            Window_Create createWindow = new Window_Create();
            createWindow.Show();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        

        private void Window_Create_Closed(object sender, System.EventArgs e)
        {
            isRunning = false;
        }
        
    }
}
