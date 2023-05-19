
public class Program
{
    public static void Main(string[] args)
    {
        //tests
        //args = new string[] { "add",
        //    "FirstName:Man",
        //    "LastName:Doe",
        //    "Salary:100.50",
        //};
        //args = new string[] { "getall" };
        //args = new string[] { "update",
        //    "Id:3",
        //    "FirstName:Name3",
        //};
        //args = new string[] { "get",
        //    "Id:1",
        //};
        //args = new string[] { "delete",
        //    "Id:1",
        //};

        var methodName = args[0].Replace(args[0].First(), 
            args[0].First().ToString().ToUpper().First());

        const string Separator = ":";
        object[]? parameters = null;
        if(methodName == "Add")
            parameters = new object[]{
                args[1].Split(Separator)[1],
                args[2].Split(Separator)[1],
                decimal.Parse(args[3].Replace(".", ",").Split(Separator)[1]),
            };

        else if(methodName == "Update")
            parameters = new object[]{ 
                int.Parse(args[1].Split(Separator)[1]),
                args[2],
            };

        else if(methodName == "Get" || methodName == "Delete")
            parameters = new object[1] { 
                int.Parse(args[1].Split(Separator)[1])
            };
        else if(methodName == "Getall")
            methodName = methodName.Replace(methodName[3],
                methodName[3].ToString().ToUpper().First());

        StafferAPI stafferAPI = new StafferAPI();
        stafferAPI.GetType()
            ?.GetMethod(methodName)
            ?.Invoke(stafferAPI, parameters);
    }
}
public class StafferCollection
{
    [JsonProperty("Staffers")]
    public Staffer[]? Staffers { get; set; }
}

public class StafferAPI
{
    private const string FileName = "staffers.json";
    private const string Separator = ":";
    private const int InitialId = 1;
    public async void Add(string firstName, string lastName, decimal salary)
    {
        try
        {
            var id = GetNextIdAsync();
            var staffer = new Staffer((int)id, firstName, lastName, salary);
            var staffers = ReadStaffers();
            staffers.Add(staffer);
            await WriteStaffersAsync(staffers);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private int GetNextIdAsync()
    {
        var staffers = ReadStaffers();
        return staffers.Any() ? staffers.Last().Id + 1 : InitialId;
    }

    public async Task Update(int id, string updatingString)
    {
        try 
        { 
            var (staffers, staffer) = GetStaffersAndFoundStaffer(id);
            var strings = updatingString.Split(Separator);
            var index = staffers.IndexOf(staffer);
            var prop = staffer
                ?.GetType()
                ?.GetProperty(strings[0]);
            prop?.SetValue(staffer, strings[1]);

            staffers[index] = new Staffer(
                id, 
                staffer.FirstName,
                staffer.LastName,
                staffer.SalaryPerHour);
            await WriteStaffersAsync(staffers);
        }
        catch (Exception ex) when(
            ex is AmbiguousMatchException ||
            ex is KeyNotFoundException)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public void Get(int id)
    {
        try
        {
            var staffer = GetStaffer(id);
            Console.WriteLine(staffer);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task Delete(int id)
    {
        try
        {
            var (staffers, staffer) = GetStaffersAndFoundStaffer(id);
            staffers.Remove(staffer);
            await WriteStaffersAsync(staffers);
        }
        catch(Exception ex) 
        {
            Console.WriteLine(ex.Message);
        }
    }

    private (List<Staffer> staffers, Staffer staffer)GetStaffersAndFoundStaffer(int id)
    {
        var staffer = GetStaffer(id);
        var staffers = ReadStaffers();
        return (staffers.ToList(), staffer);
    }
    private Staffer GetStaffer(int id)
    {
        var staffers = ReadStaffers();
        var staffer = staffers.FirstOrDefault(s => s?.Id == id);
        if (staffer is null)
        {
            Console.WriteLine($"Staffer #{id} not found");
            throw new KeyNotFoundException();
        }
        return staffer;
    }

    private static async Task WriteStaffersAsync(List<Staffer> staffers)
    {
        await File.WriteAllTextAsync(
            FileName, JsonConvert.SerializeObject(new StafferCollection
            {
                Staffers = staffers.ToArray()
            }));
    }

    public void GetAll()
    {
        var staffers = ReadStaffers();
        foreach (var staffer in staffers)
            Console.WriteLine(staffer);
    }

    private List<Staffer> ReadStaffers()
    {
        var staffers = new StafferCollection();
        string content = "";
        content = File.ReadAllText(FileName);
        staffers = JsonConvert.DeserializeObject<StafferCollection>(content);
        if(staffers?.Staffers is null)
        {
            Console.WriteLine("Error! Collection is empty!");
            throw new NullReferenceException();
        }

        return staffers.Staffers.ToList();
    }
}

