﻿ReSharper Postfix Completion plugin
-----------------------------------

The idea is to prevent caret jumps backwards while typing C# code.

[See it in action](http://screencast.com/t/zqMDGTMDqhp)

#### Download

Pre-release package version is available in in ReSharper 8.0 Extension Manager gallery.
ReSharper 7.1 version will came shortly.

#### Available templates

Iterting over all kinds of collections, reverse iteration:

![foreach](/Content/foreach.png)

<table>
  <tr>
    <th>Name</th>
    <th>Example</th>
  </tr>
  <tr>
    <td>.arg</td>
    <td>`expr` => `F(expr)`</td>
  </tr>
  <tr>
    <td> **.await** </td>
    <td> `expr` => `await expr` </td>
  </tr>
</table>

* .arg
* **.await**
* .cast
* .null
* .notnull
* .foreach
* .for
* .forr
* .if
* .ifnot
* .not
* .field
* .prop
* .var
* .not
* .new
* .paren
* .return
* .throw
* .using
* .while

TODO:

* .lock
* work on statements?
** .try
** .if


// TODO: other templates

#### Feedback

Feel free to post any issues or feature requests in [JetBrains YouTrack](http://youtrack.jetbrains.com/issues/RSPL) (select *"PostfixCompletion"* subsystem).

Or contact me directly by email: *alexander.shvedov[at]jetbrains.com*