using EntityFrameworkApp;
using Microsoft.EntityFrameworkCore;

var options = new DbContextOptionsBuilder<SampleDbContext>()
    .UseInMemoryDatabase("SampleDb")
    .Options;
var context = new SampleDbContext(options);
var user = context.Users.SingleOrDefault(x => x.UserId == new UserId(1));

Console.WriteLine("Hello, World!");