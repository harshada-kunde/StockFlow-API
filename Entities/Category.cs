namespace StockFlow.API.Entities;

public class Category
{

    public int Id {get;set;} //Db will increment id for each record
    public string Name {get;set;}
    public DateTime CreatedOn {get;set;}
    public string CreatedBy {get;set;}

    //Navigation Property
    //public List<Product>Products{get;set;}= new();

}
//EF Core has built-in conventions — rules it follows automatically without you writing any configuration
//Q. How does EF Core know which property is the primary key?
//answer : "By convention, EF Core automatically treats a property named Id or ClassName+Id as the primary key. If it's an int, it automatically adds IDENTITY for auto-increment. We can override this using Fluent API or Data Annotations if needed."
