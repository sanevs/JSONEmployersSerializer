
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
        //    "Id:1",    
        //    "FirstName:M22",
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
        object[] parameters = null;
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
    public Staffer[] Staffers { get; set; }
}

public class StafferAPI
{
    private const string FileName = "staffers.json";
    private const string Separator = ":";
    private const int InitialId = 1;
    public async void Add(string firstName, string lastName, decimal salary)
    {
        var id = await GetNextIdAsync();
        var staffer = new Staffer((int)id, firstName, lastName, salary);
        var staffers = await ReadStaffersAsync();
        staffers?.Add(staffer);
        await WriteStaffersAsync(staffers);
    }

    private async Task<int?> GetNextIdAsync()
    {
        var staffers = await ReadStaffersAsync();
        return staffers.Any() ? staffers.Last()?.Id + 1 : InitialId;
    }

    public async Task Update(int id, string updatingString)
    {
        try 
        { 
            var staffersTouple = await GetStaffersAndFoundStafferAsync(id);
            var strings = updatingString.Split(Separator);
            var index = staffersTouple.staffers.IndexOf(staffersTouple.staffer);
            var prop = staffersTouple.staffer
                ?.GetType()
                ?.GetProperty(strings[0]);
            prop?.SetValue(staffersTouple.staffer, strings[1]);

            staffersTouple.staffers[index] = new Staffer(
                id, 
                staffersTouple.staffer.FirstName,
                staffersTouple.staffer.LastName,
                staffersTouple.staffer.SalaryPerHour);
            await WriteStaffersAsync(staffersTouple.staffers);
        }
        catch (Exception ex) when(
            ex is AmbiguousMatchException ||
            ex is KeyNotFoundException)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task Get(int id)
    {
        Staffer? staffer = await GetNullableStafferAsync(id);
        if (staffer != null)
            Console.WriteLine(staffer);
    }

    public async Task Delete(int id)
    {
        try
        {
            var staffersTouple = await GetStaffersAndFoundStafferAsync(id);
            staffersTouple.staffers?.Remove(staffersTouple.staffer);
            await WriteStaffersAsync(staffersTouple.staffers);
        }
        catch(KeyNotFoundException ex) 
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async Task<(List<Staffer?>? staffers, Staffer? staffer)>
        GetStaffersAndFoundStafferAsync(int id)
    {
        var staffer = await GetNullableStafferAsync(id);
        if (staffer is null)
            throw new KeyNotFoundException();
        var staffers = await ReadStaffersAsync();
        return (staffers?.ToList(), staffer);
    }
    private async Task<Staffer?> GetNullableStafferAsync(int id)
    {
        var staffers = await ReadStaffersAsync();
        var staffer = staffers?.FirstOrDefault(s => s?.Id == id);
        if (staffer is null)
            Console.WriteLine($"Staffer #{id} not found");
        return staffer;
    }

    private async Task WriteStaffersAsync(List<Staffer?> staffers)
    {
        await File.WriteAllTextAsync(
            FileName, JsonConvert.SerializeObject(new StafferCollection
            {
                Staffers = staffers?.ToArray()
            }));
    }

    public async Task GetAll()
    {
        var staffers = await ReadStaffersAsync();
        foreach (var staffer in staffers)
            Console.WriteLine(staffer);
    }

    private async Task<List<Staffer?>> ReadStaffersAsync()
    {
        var staffers = new StafferCollection { Staffers = new Staffer[0] };
        string content = "";
        try
        {
            content = File.ReadAllText(FileName);
        }
        catch (FileNotFoundException)
        { 
        }
        staffers = JsonConvert.DeserializeObject<StafferCollection>(content);
        return staffers?.Staffers.ToList();
    }
}

