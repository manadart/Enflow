![](https://dl.dropboxusercontent.com/u/35984366/enflow_logo3_m.png)

Enflow is a simple library for workflows and state/business rules. It is a potential replacement for the _Unit of Work_ pattern popular in MVC applications, particularly where model state validation must accompany units of work.

Usage is not limited to MVC. Enflow is a [Portable Class Library](http://msdn.microsoft.com/en-us/library/gg597391.aspx) (PCL) and works across multiple platforms. For more information, including usage in _Xamarin.Android_ and _Xamarin.iOS_, see [this blog post](http://slodge.blogspot.sk/2012/12/cross-platform-winrt-monodroid.html) (and [update](http://slodge.blogspot.ca/2013/04/my-current-pcl-setup-in-visual-studio.html)) by Stuart Lodge ([@slodge](https://twitter.com/slodge)). _Note: Currently experiencing a problem targeting Xamarin.Android and Xamarin.iOS. Working to resolve._

Enflow is available via [NuGet](https://nuget.org/packages/Enflow/).

### Models

Note: It is no longer necessary to mark types with the ```IModel<T>``` interface. This has been removed - Enflow is now much more versatile.

### State Rules

Create rules based on any type and use the fluent API to create composite rules from atomic constituents.
```csharp

public class Employee
{
    public string Name { get; set; }
    public string Department { get; set; }
    public string Salary { get; set; }
}

public class MaxSalaryRule : StateRule<Employee>
{
    public override Expression<Func<Employee, bool>>
    {
        get { return candidate => candidate.Salary < 40000; }
    }
}

public class InHrDepartmentRule : StateRule<Employee>
{
    public override Expression<Func<Employee, bool>>
    {
        get { return candidate => candidate.Department == "Human Resources"; }
    }
}

// Compose a new rule from the others and describe it.
var salaryRaiseRule = new MaxSalaryRule()
    .And(new InHrDepartmentRule())
    .Describe("Employee must be in the HR deparment and have a salary less than $40,000.");

// Candidates can then be checked against a rule to see if they satisfy it.
var isEligible = salaryRaiseRule.IsSatified(someEmployee);

// A rule can also be used to filter an IQueryable via the Predicate property.
// Depending on the actual expression, this will also work with LINQ to Entities.
var eligibleEmployees = Employees.Where(salaryRaiseRule.Predicate);
```

### Workflows

This is our new _Unit of Work_. Instantiate, passing in the rule to be validated. If the rule validation fails, a _StateRuleException_ will be thrown with the rule description as the message.
```csharp
// Example incorporating a repository pattern.
public class ApplySalaryRaise : Workflow<Employee>
{
    private readonly IRepository<Employee> _repository;

    public ApplySalaryRaise(IStateRule<Employee> rule, IRepository<Employee> repository) : base(rule)
    {
        _repository = repository;
    }

    protected override Employee ExecuteWorkflow(Employee candidate)
    {
        candidate.Salary += 5000;
        _repository.Update(candidate);
        return candidate;
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

// Input candidate will be granted the salary raise.
salaryRaiseWorflow.Execute(eligibleEmployee); 

// This will throw a StateRuleException.
salaryRaiseWorflow.Execute(ineligibleEmployee);
```

### Flowable

Flowable is a type amplification wrapper that allows fluent, declaritive use of Enflow. 

```csharp
// It is possible to chain workflows using flowable.
// This would apply two pay increases as long as the salary remained under $40k.
eligibleEmployee
    .AsFlowable()
    .Flow(salaryRaiseWorflow)
    .Flow(salaryRaiseWorflow);

// State rule checks can also be applied directly to flowables.
var canThisPersonGetMore = eligibleEmployee
    .AsFlowable()
    .Flow(salaryRaiseWorflow)
    .Satisfies(salaryRaiseRule);
```

An ```IWorkflow<T>``` returns _T_ from its call to _Execute_. ```IWorkflow<T, U>``` allows chaining together sequences of workflows that pass different types between them. This means complex workflows can be expressed as compositions of smaller ones, increasing modularity, re-use and testability.

```csharp
public class Department
{
    public string Name { get; set; }
    public int EmployeeCount { get; set; }
    public string Building { get; set; }
}

// These examples have the repository/persistence code omitted for clarity.

public class MoveHrPersonToNewDepartment : Workflow<Employee, Department>
{
    public Department Destination { get; set; }

    protected override Department ExecuteWorkflow(Employee candidate)
    {
        candidate.Department = Destination.Name;
        Destination.EmployeeCount++;
        return Destination;
    }
}

public class MoveToTheNewOffice : Workflow<Department>
{
    protected override Department ExecuteWorkflow(Department candidate)
    {
        candidate.Building = "Uptown Office";
        return candidate;
    }
}

var moveToBoardWorkflow = new MoveHrPersonToNewDepartment(new InHrDepartmentRule())
    {
        // Something like this would come from the DB in a less contrived example, of course.
        Department = new Department 
            { 
                Name = "Board of Directors", 
                EmployeeCount = 5,
                Building = "Downtown Office"
            }
    };

var moveDeptOfficeWorkflow = new MoveToTheNewOffice();

var employeeLocation = eligibleEmployee
    .AsFlowable()
    .Flow(salaryRaiseWorflow)
    .Flow(moveToBoardWorkflow)
    .Flow(moveDeptOfficeWorkflow)
    .Value
    .Building; // "Uptown Office"
```

### The Workflow Factory

Note: The workflow factory has been removed from Enflow.

### License

Unless otherwise specified, all content in this repository is licensed as follows.

The MIT License (MIT)

__Copyright (c) 2013 Joseph Phillips__

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
