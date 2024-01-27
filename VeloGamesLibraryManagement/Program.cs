using System.Globalization;

class LibraryManagementSystem
{
    #region Fields
    private static long _lastISBN;
    private static readonly string _booksFileName = "books.txt";
    private static readonly string _borrowedBooksFileName = "borrowed_books.txt";
    #endregion

    #region Main Method
    static void Main()
    {
        LibraryController();
    }
    #endregion

    #region Constructors
    static LibraryManagementSystem()
    {
        _lastISBN = LoadLastISBN();
    }
    #endregion

    #region Methods
    static void LibraryController()
    {
        List<Book> libraryBooks = LoadBooks();
        List<BorrowedBook> borrowedBooks = LoadBorrowedBooks();

        while (true)
        {
            Console.WriteLine("Library Management System");
            Console.WriteLine("1. Add a Book");
            Console.WriteLine("2. Display All Books");
            Console.WriteLine("3. Search for a Book");
            Console.WriteLine("4. Borrow a Book");
            Console.WriteLine("5. Return a Book");
            Console.WriteLine("6. Show Overdue Books");
            Console.WriteLine("7. Clear The Console");
            Console.WriteLine("8. Exit");
            Console.Write("Enter your choice: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddBook(libraryBooks);
                    SaveBooks(libraryBooks);
                    break;
                case "2":
                    DisplayBooks(libraryBooks);
                    break;
                case "3":
                    SearchBook(libraryBooks);
                    break;
                case "4":
                    BorrowBook(libraryBooks, borrowedBooks);
                    SaveBooks(libraryBooks);
                    SaveBorrowedBooks(borrowedBooks);
                    break;
                case "5":
                    ReturnBook(libraryBooks, borrowedBooks);
                    SaveBooks(libraryBooks);
                    SaveBorrowedBooks(borrowedBooks);
                    break;
                case "6":
                    ShowOverdueBooks(borrowedBooks);
                    break;
                case "7":
                    Console.Clear();
                    break;
                case "8":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    static void AddBook(List<Book> libraryBooks)
    {
        Console.Write("Enter Book Title: ");
        string title = Console.ReadLine();

        Console.Write("Enter Author: ");
        string author = Console.ReadLine();

        var existingBook = libraryBooks.Find(book => book.Title.ToLower() == title.ToLower() && book.Author.ToLower() == author.ToLower());
        if (existingBook != null)
        {
            existingBook.Copies++;
            Console.WriteLine($"Existing book found. Copies increased. Total copies: {existingBook.Copies}");
        }
        else
        {
            long isbn = GenerateISBN();
            short copies = 1; 

            Book newBook = new Book { ISBN = isbn, Title = title, Author = author, Copies = copies };
            libraryBooks.Add(newBook);

            Console.WriteLine($"Book added successfully! ISBN: {isbn}");
        }
    }

    static void DisplayBooks(List<Book> libraryBooks)
    {
        if (libraryBooks.Count == 0)
        {
            Console.WriteLine("No books available in the library.");
        }
        else
        {
            Console.WriteLine("List of Books:");
            foreach (var book in libraryBooks)
            {
                Console.WriteLine($"ISBN: {book.ISBN}, {book.Title} by {book.Author}");
            }
        }
    }

    static void SearchBook(List<Book> libraryBooks)
    {
        Console.Write("Enter the title or author to search: ");
        string searchTerm = Console.ReadLine().ToLower();

        var results = libraryBooks.FindAll(book => book.Title.ToLower().Contains(searchTerm) || book.Author.ToLower().Contains(searchTerm));

        if (results.Count == 0)
        {
            Console.WriteLine("No matching books found.");
        }
        else
        {
            Console.WriteLine("Matching Books:");
            foreach (var book in results)
            {
                Console.WriteLine($"ISBN: {book.ISBN}, {book.Title} by {book.Author}");
            }
        }
    }

    static void BorrowBook(List<Book> libraryBooks, List<BorrowedBook> borrowedBooks)
    {
        Console.Write("Enter the ISBN of the book you want to borrow: ");
        long isbnToBorrow = long.Parse(Console.ReadLine());

        Book selectedBook = libraryBooks.Find(book => book.ISBN == isbnToBorrow);

        if (selectedBook != null && selectedBook.Copies > 0)
        {
            Console.Write("Enter the due date (yyyy-MM-dd) for returning the book: ");
            DateTime dueDate;
            if (DateTime.TryParse(Console.ReadLine(), out dueDate))
            {
                selectedBook.Copies--;

                BorrowedBook borrowedBook = new()
                {
                    ISBN = selectedBook.ISBN,
                    Title = selectedBook.Title,
                    BorrowDate = DateTime.Now,
                    DueDate = dueDate
                };
                borrowedBooks.Add(borrowedBook);

                Console.WriteLine($"Book '{selectedBook.Title}' borrowed successfully. Due date: {dueDate.ToShortDateString()}. Remaining copies: {selectedBook.Copies}");
            }
            else
            {
                Console.WriteLine("Invalid date format. Borrow failed.");
            }
        }
        else if (selectedBook != null && selectedBook.Copies == 0)
        {
            Console.WriteLine($"Book '{selectedBook.Title}' is currently out of stock.");
        }
        else
        {
            Console.WriteLine("Book not found with the provided ISBN.");
        }
    }

    static void ReturnBook(List<Book> libraryBooks, List<BorrowedBook> borrowedBooks)
    {
        Console.Write("Enter the ISBN of the book you want to return: ");
        long isbnToReturn = long.Parse(Console.ReadLine());

        BorrowedBook borrowedBook = borrowedBooks.Find(book => book.ISBN == isbnToReturn);

        if (borrowedBook != null)
        {
            Book selectedBook = libraryBooks.Find(book => book.ISBN == isbnToReturn);
            selectedBook.Copies++;

            borrowedBooks.Remove(borrowedBook);

            Console.WriteLine($"Book '{selectedBook.Title}' returned successfully. Remaining copies: {selectedBook.Copies}");
        }
        else
        {
            Console.WriteLine("Book not found with the provided ISBN or it is not currently borrowed.");
        }
    }

    static void ShowOverdueBooks(List<BorrowedBook> borrowedBooks)
    {
        Console.WriteLine("Overdue Books:");
        foreach (var borrowedBook in borrowedBooks)
        {
            if (borrowedBook.DueDate < DateTime.Now)
            {
                Console.WriteLine($"ISBN: {borrowedBook.ISBN}, Title: {borrowedBook.Title}, Due Date: {borrowedBook.DueDate.ToShortDateString()}");
            }
        }
    }

    static List<Book> LoadBooks()
    {
        List<Book> books = new();

        if (File.Exists(_booksFileName))
        {
            string[] lines = File.ReadAllLines(_booksFileName);
            foreach (var line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 4)
                {
                    Book book = new()
                    {
                        ISBN = long.Parse(parts[0]),
                        Title = parts[1],
                        Author = parts[2],
                        Copies = short.Parse(parts[3])
                    };
                    books.Add(book);
                }
            }
        }

        return books;
    }

    static List<BorrowedBook> LoadBorrowedBooks()
    {
        List<BorrowedBook> borrowedBooks = new();

        if (File.Exists(_borrowedBooksFileName))
        {
            string[] lines = File.ReadAllLines(_borrowedBooksFileName);
            foreach (var line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 4)
                {
                    BorrowedBook borrowedBook = new()
                    {
                        ISBN = long.Parse(parts[0]),
                        Title = parts[1],
                        BorrowDate = DateTime.ParseExact(parts[2], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                        DueDate = DateTime.ParseExact(parts[3], "yyyy-MM-dd", CultureInfo.InvariantCulture)
                    };
                    borrowedBooks.Add(borrowedBook);
                }
            }
        }

        return borrowedBooks;
    }

    static long LoadLastISBN()
    {
        long last = 1000000000000;
        if (File.Exists(_booksFileName))
        {
            string[] lines = File.ReadAllLines(_booksFileName);
            foreach (var line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length > 0)
                {
                    long isbn;
                    if (long.TryParse(parts[0], out isbn) && isbn > last)
                    {
                        last = isbn;
                    }
                }
            }
        }
        return last;
    }

    static void SaveBooks(List<Book> libraryBooks)
    {
        using (StreamWriter writer = new(_booksFileName))
        {
            foreach (var book in libraryBooks)
            {
                writer.WriteLine($"{book.ISBN},{book.Title},{book.Author},{book.Copies}");
            }
        }
    }

    static void SaveBorrowedBooks(List<BorrowedBook> borrowedBooks)
    {
        using (StreamWriter writer = new(_borrowedBooksFileName))
        {
            foreach (var borrowedBook in borrowedBooks)
            {
                writer.WriteLine($"{borrowedBook.ISBN},{borrowedBook.Title},{borrowedBook.BorrowDate.ToString("yyyy-MM-dd HH:mm:ss")},{borrowedBook.DueDate.ToString("yyyy-MM-dd")}");
            }
        }
    }

    static long GenerateISBN()
    {
        _lastISBN++;
        return _lastISBN;
    }

    #endregion
}

#region Book Classes
class Book
{
    public long ISBN { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public short Copies { get; set; } = 1;
}


class BorrowedBook
{
    public long ISBN { get; set; }
    public string Title { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
}

#endregion
