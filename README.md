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

When using Enflow in small MVC applications it is acceptable to inject workflows directly into controllers. However if a controller depends on multiple workflows, consider the _WorkflowFactory_.

#### Stand Alone Example

```csharp
// Not strictly necessary, but allows intellisense for named resolutions.
public static class Workflows
{
    public const string SalaryRaise = "Salary Raise";
}

public class StandAloneWorkflowFactory : WorkflowFactory
{
    public StandAloneWorkflowFactory()
    {
        Register(Workflows.SalaryRaise, () => 
            new ApplySalaryRaise(new MaxSalaryRule()
                .And(new InHrDepartmentRule())
                .Describe("Employee must be in the HR deparment and have a salary less than $40,000."), 
            new EmployeeRepository()));
        
        // Register other workflows...
    }
}

// Controllers requiring the factory will inherit from this.
public class WorkflowController : Controller
{
    protected readonly IWorkflowFactory WorkflowFactory;

    public WorkflowController(IWorkflowFactory workflowFactory)
    {
        WorkflowFactory = workflowFactory;
    }
}

public class WorkflowControllerFactory : DefaultControllerFactory
{
    protected override IController GetControllerInstance(RequestContext context, Type controllerType)
    {
        if (typeof(WorkflowController).IsAssignableFrom(controllerType))
            return (WorkflowController)Activator.CreateInstance(controllerType, new object[] { new StandAloneWorkflowFactory() });

        return base.GetControllerInstance(context, controllerType);
    }
}

// In Application_Start()
ControllerBuilder.Current.SetControllerFactory(new WorkflowControllerFactory());

// Then when a workflow is required in a controller method...
var workflow = WorkflowFactory.Get<Employee>(Workflows.SalaryRaise);
```

#### Autofac Example

```csharp
// When combined with an IoC container, the factory is just an indirection.
// It allows resolution via the container without resorting to the Service Locator antipattern.
public class AutofacWorkflowFactory : IWorkflowFactory
{
    private readonly IComponentContext _container;

    public AutofacWorkflowFactory(IComponentContext container)
    {
        _container = container;
    }

    public IWorkflow<T> Get<T>(string name) where T : IModel<T>
    {
        _container.ResolveNamed<T>(name);
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

        // Register model binders etc...
        // Register repositories if you're using such a pattern.

        const string salaryAndDeptRule = "SalaryAndDeptRule";

        builder.Register(c => new MaxSalaryRule()
            .And(new InHrDepartmentRule())
            .Describe("Employee must be in the HR deparment and have a salary less than $40,000."))
                .Named<IBusinessRule<Employee>>(salaryAndDeptRule)
                .InstancePerHttpRequest();

        builder.Register(c => new ApplySalaryRaise(c.ResolveNamed<IBusinessRule<Employee>>(salaryAndDeptRule), c.Resolve<IRepository<Employee>>()))
            .Named<IWorkflow<Employee>>(Workflows.SalaryRaise)
            .InstancePerHttpRequest();

        builder.Register(r => new AutofacWorkflowFactory(container))
            .As<IWorkflowFactory>();
            .InstancePerHttpRequest();

        // Other registrations...
    }
}
```

### License

Unless otherwise specified, all content in this repository is licensed as follows.

The MIT License (MIT)

Copyright (c) 2013 Joseph Phillips

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
