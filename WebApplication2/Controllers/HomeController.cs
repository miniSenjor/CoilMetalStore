using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{

    public class HomeController : Controller
    {
        public string Index()
        {
            return "index";
        }
        public string Per(Person person)
        {
            return $"Your name: {person.Name} Your age: {person.Age}";
        }
        public Message Mes()
        {
            return new Message { s = "message" };
        }
    }
    public class Message
    {
        public string s { get; set; }
    }
    public record class Person(string Name, int Age);
}
