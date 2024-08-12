using Microsoft.EntityFrameworkCore;

public class ReproContext : DbContext
{
    public ReproContext(DbContextOptions<ReproContext> options) : base(options) { }

    public DbSet<Entity> Entities { get; set; }
    public DbSet<EntityType> EntityTypes { get; set; }
    public DbSet<Tag> Tags { get; set; }
}

public class Entity
{
    public int ID { get; set; }
    public EntityType Type { get; set; }
}

public class EntityType
{
    public int ID { get; set; }
    public IEnumerable<Tag> Tags { get; set; }
}

public class Tag
{
    public int ID { get; set; }
}

public class DtoEntity
{
    public int ID { get; set; }
    public DtoEntityType Type { get; set; }
}

public class DtoEntityType
{
    public int ID { get; set; }
    public IEnumerable<DtoTag> Tags { get; set; }
}

public class DtoTag
{
    public int ID { get; set; }
}

public class FinalResult
{
    public decimal Value { get; set; }
}

public class Program
{
    public static void Main(string[] args)
    {
        var builder = new DbContextOptionsBuilder<ReproContext>();

        builder.UseInMemoryDatabase("test");

        using (var context = new ReproContext(builder.Options))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Entities.Add(new Entity
            {
                ID = 1,
                Type = new EntityType
                {
                    ID = 1,
                    Tags = new List<Tag>()
                    {
                        new Tag() { ID = 2 }
                    }
                }
            });
            context.SaveChanges();
        }

        using (var context = new ReproContext(builder.Options))
        {

            //Works
            var test1 = context.Entities
                .Select(f => new DtoEntity()
                {
                    ID = f.ID,
                    Type = new DtoEntityType()
                    {
                        ID = f.Type.ID,
                        Tags = f.Type.Tags.Select(t => new DtoTag() { ID = t.ID })
                    }
                })
                .Select(f => new FinalResult
                {
                    Value = f.Type.Tags.Count()
                })
                .Single();
            Console.WriteLine(test1.Value);

            //Exception
            var test2 = context.Entities
                .Select(f => new DtoEntity()
                {
                    ID = f.ID,
                    Type = f.Type == null ? null : new DtoEntityType()
                    {
                        ID = f.Type.ID,
                        Tags = f.Type.Tags.Select(t => new DtoTag() { ID = t.ID })
                    }
                })
                .Select(f => new FinalResult
                {
                    Value = f.Type.Tags.Count()
                })
                .Single();
            Console.WriteLine(test2.Value);

        }
    }

}