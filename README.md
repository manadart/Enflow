Enflow
======

Enflow is a simple library for workflows and business rules. It is an ideal replacement for the _Unit of Work_ pattern popular in MVC applications, particularly where model state validation must accompany units of work.

### Models

Just mark your DTO/POCO models with the Enflow model interface.
```csharp
public class Employee : IModel<Employee>
{
	public string Name { get; set; }
    public string Department { get; set; }
    public string Salary { get; set; }
}
```

### Business Rules

Create business rules based on your models and use the fluent API to create composite rules from atomic constituents.
```csharp
public class MaxSalaryRule : BusinessRule<Employee>
{
    public override bool IsSatisfied(Employee candidate)
    {
        return candidate.Salary < 40000;
    }
}

public class InHrDepartmentRule : BusinessRule<Employee>
{
    public override bool IsSatisfied(Employee candidate)
    {
        return candidate.Department == "Human Resources";
    }
}

// Compose a new rule from the others and describe it.
var salaryRaiseRule = new MaxSalaryRule()
    .And(new InHrDepartmentRule())
    .Describe("Employee must be in the HR deparment and have a salary less than $40,000.");
```

### Workflows

This is our new _Unit of Work_. Instantiate, passing in the rule to be validated. If the rule validation fails, a _BusinessRuleException_ will be thrown with the rule description as the message.
```csharp
// Example incorporating a repository pattern.
public class ApplySalaryRaise : Workflow<Employee>
{
    private readonly IRepository<Employee> _repository;

    public ApplySalaryRaise(IBusinessRule<Employee> rule, IRepository<Employee> repository)
        : base(rule)
    {
        _repository = repository;
    }

    protected override void ExecuteWorkflow(Employee candidate)
    {
        candidate.Salary += 5000;
        _repository.Update(candidate);
    }
}

// Some example candidates for our workflow.
var eligibleEmployee = new Employee
    {
        Name = "John Smith",
        Department = "Human Resources",
        Salary = 39000
    };

var ineligibleEmployee = new Employee
    {
        Name = "Tye Tarse",
        Department = "Board of Directors",
        Salary = 1000000
    };

// Give the candidate employee a $5000 raise if they're in HR and they earn less than $40k.
var salaryRaiseWorflow = new ApplySalaryRaise(salaryRaiseRule, new EmployeeRepository());

// Will be granted the salary raise.
salaryRaiseWorflow.Execute(elligibleEmployee); 

// Will throw a BusinessRuleException.
salaryRaiseWorflow.Execute(inelligibleEmployee); 

```

### The Workflow Factory

When using Enflow in small MVC applications it's acceptable to inject workflows directly into controllers. However if a controller depends on multiple workflows, consider the _WorkflowFactory_.

#### Stand Alone

```csharp
// Not strictly necessary, but allows intellisense for named resolutions.
public static class Workflows
{
    public const string SalaryRaise = "Salary Raise";
}

// TBC.

```

#### Autofac

```csharp
// When combined with an IoC container, the factory really just abstracts away said container.
public class MyWorkflowFactory : WorkflowFactory
{
    public MyWorkflowFactory(IComponentContext container)
    {
        Register(Workflows.SalaryRaise, () => container.ResolveNamed<IWorkflow<Employee>>(Workflows.SalaryRaise));
        // Register other workflows.
    }
}

public class MvcApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
        var builder = new ContainerBuilder();
        builder.RegisterControllers(typeof(MvcApplication).Assembly);
        var container = builder.Build();
        DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

        // Register model binders et al.

        var salaryRaiseRule = new MaxSalaryRule()
            .And(new InHrDepartmentRule())
            .Describe("Employee must be in the HR deparment and have a salary less than $40,000.");

        builder.Register(r => new ApplySalaryRaise(salaryRaiseRule, new EmployeeRepository()))
            .Named<IWorkflow<Employee>>(Workflows.SalaryRaise)
            .InstancePerHttpRequest();

        builder.Register(r => new MyWorkflowFactory(container)).As<IWorkflowFactory>();

        // Other registrations.
    }
}

// TBC.

```