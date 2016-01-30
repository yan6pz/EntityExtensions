# EntityExtensions
<h3>Providing library for bulk update of several entity objects </h3>

There is an extension method of entity class which has
<pre><code> Expression < Func < T, P>></code></pre> 
argument for selector which property to update.
Another argument of that method is a dictionary where the desired values for update should be placed(key:id of the record and value: specific value) Via reflection of the <b>selector</b> is getted which property should be updated(selector is added as lambda expression) and dynamically temporary table for storing values is created.
Example of usage:
<pre><code>
    var paramMap = new Dictionary<string, string>()
                      {
                          {"1", "updated1"},
                          {"2", "updated2"},
                          {"3", "updated3"},
                          {"4", "updated4"}
                      };
    entity.ExecuteScalarBulk(p => p.LastName, paramMap);
    </code></pre>
