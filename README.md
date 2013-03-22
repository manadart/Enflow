Enflow
======

Enflow is a simple library for workflows and business rules. It is an ideal replacement for the _Unit of Work_ pattern popular in MVC applications, particularly where model state validation must accompany units of work.

Usage is not limited to MVC. Enflow is a [Portable Class Library](http://msdn.microsoft.com/en-us/library/gg597391.aspx) (PCL) and works across multiple platforms. For more information, including usage in _Mono for Android_ and _MonoTouch_, have a read of [this blog post](http://slodge.blogspot.sk/2012/12/cross-platform-winrt-monodroid.html) by Stuart Lodge ([@slodge](https://twitter.com/slodge)).

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
        Salary = 10000;
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
salaryRaiseWorflow.Execute(eligibleEmployee); 

// Will throw a BusinessRuleException.
salaryRaiseWorflow.Execute(ineligibleEmployee);

// It is also possible to chain multiple workflows using the fluent API.
// This would apply two pay increases as long as the salary remained under $40k.
eligibleEmployee
    .Flow(salaryRaiseWorflow)
    .Flow(salaryRaiseWorflow);

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
        Register(Workflows.SalaryRaise, () => new ApplySalaryRaise(
            new MaxSalaryRule()
                .And(new InHrDepartmentRule())
                .Describe("Employee must be in the HR deparment and have a salary less than $40,000."), 
            new EmployeeRepository()));
        
        // Register other workflows...
    }
}

// Controllers requiring the factory will inherit from this.
public abstract class WorkflowController : Controller
{
    protected readonly IWorkflowFactory WorkflowFactory;

    protected WorkflowController(IWorkflowFactory workflowFactory)
    {
        WorkflowFactory = workflowFactory;
    }
}

public class WorkflowControllerFactory : DefaultControllerFactory
{
    protected override IController GetControllerInstance(RequestContext context, Type controllerType)
    {
        if (typeof(WorkflowController).IsAssignableFrom(controllerType))
            return (WorkflowController)Activator
                .CreateInstance(controllerType, new object[] { new StandAloneWorkflowFactory() });

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
// When combined with an IoC container, the factory is just an indirection for MVC's inbuilt Service Locator.
public class AutofacWorkflowFactory : IWorkflowFactory
{
    public IWorkflow<T> Get<T>(string name) where T : IModel<T>
    {
        return ((AutofacDependencyResolver)DependencyResolver.Current)
            .RequestLifetimeScope.ResolveNamed<IWorkflow<T>>(name);
    }
}

public class MvcApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
        var builder = new ContainerBuilder();
        builder.RegisterControllers(typeof(MvcApplication).Assembly);

        // Register model binders etc...
        // Register repositories...

        const string salaryAndDeptRule = "SalaryAndDeptRule";

        builder.Register(c => new MaxSalaryRule()
            .And(new InHrDepartmentRule())
            .Describe("Employee must be in the HR deparment and have a salary less than $40,000."))
                .Named<IBusinessRule<Employee>>(salaryAndDeptRule)
                .InstancePerHttpRequest();

        builder.Register(c => new ApplySalaryRaise(
            c.ResolveNamed<IBusinessRule<Employee>>(salaryAndDeptRule), 
            c.Resolve<IRepository<Employee>>()))
                .Named<IWorkflow<Employee>>(Workflows.SalaryRaise)
                .InstancePerHttpRequest();

        builder.Register(c => new AutofacWorkflowFactory())
            .As<IWorkflowFactory>()
            .InstancePerHttpRequest();

        DependencyResolver.SetResolver(new AutofacDependencyResolver(builder.Build()));

        // Other setup...
    }
}
```

### License

Unless otherwise specified, all content in this repository is licensed as follows.

The MIT License (MIT)

__Copyright (c) 2013 Joseph Phillips__

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
