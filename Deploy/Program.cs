var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/books", () =>
{
    return Results.Ok(Book.All);
})
.WithName("GetBooks")
.WithOpenApi();

app.Run();

public class Book
{
    public Book(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; set; }
    public string Name { get; set; }

    public static List<Book> All = [
        new Book(1, "Things Fall Apart"),
        new Book(2, "Lord Of The Rings"),
        new Book(3, "Romeo and Juliet")
        ];
}