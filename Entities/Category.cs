namespace StockFlow.API.Entities;
public class Category
{
    public int Id {get;set;}
    public string Name {get;set;}
    public DateTime CreatedOn {get;set;}
    public string CreatedBy {get;set;}

    //Navigation Property
    public List<Product>Products{get;set;}= new();

}