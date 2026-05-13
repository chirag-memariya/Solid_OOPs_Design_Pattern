**Yes. 100% required.**
For a method to be an **extension method**, it MUST be:

### ✔ inside a **static class**

### ✔ the method itself must be **static**

### ✔ the first parameter must have `this` keyword

Example:

```csharp
public static class StringExtensions
{
    public static string Capitalize(this string input)
    {
        ...
    }
}
```

Without `static`, it **cannot** be an extension method.

---

# 🧠 Why must it be static?

Because extension methods are just **compiler tricks**.

When you write:

```csharp
name.Capitalize();
```

The compiler translates it to:

```csharp
StringExtensions.Capitalize(name);
```

This requires:

* Method = static (so compiler can call it without object)
* Class = static (so it can’t be instantiated)

---

# ❌ Example that does NOT work

```csharp
public class BadExtension
{
    public string Capitalize(this string s) { ... }  // ❌ ERROR
}
```

You will get a compile error:

> Extension method must be defined in a non-generic static class.

---

# 🎯 Super Short Answer

**YES — Extension method MUST be static.**
Both the **class** and the **method**.

---

Here is the **clearest explanation** of *what an extension class really is* — and **why we use extension methods**.

---

# ✅ **What is an Extension Class?**

An **extension class** is simply a:

* **static class**
* containing **static extension methods**
* that “extend” an existing class **without modifying it**

Example:

```csharp
public static class StringExtensions
{
    public static string Capitalize(this string input) { ... }
}
```

This class is called an **extension class** because it *extends* `string`.

---

# 🎯 **Why do we use extension methods?**

### **1. When the original class is already in production**

The class is tested, stable, used everywhere.

Changing it is risky.

🔥 Instead of editing the original class →
you add a new method in an **extension class**.

---

### **2. When the original class is sealed / not yours**

Examples:

* `string`
* `DateTime`
* `HttpClient`
* LINQ extensions (`Where`, `Select`)

You **cannot modify** these built-in classes.

But with extension methods, you can add new behavior.

---

### **3. No need to re-test the entire old class**

Because you are NOT touching the existing class.

You add new behavior **outside** in a separate class.

Only the new method needs testing.

---

### **4. Cleaner and more readable code**

Instead of writing:

```csharp
StringHelper.Capitalize(name);
```

You write:

```csharp
name.Capitalize();
```

Feels like a natural method of the type.

---

### **5. Follow Open/Closed Principle (SOLID OCP)**

* **Open for extension**
* **Closed for modification**

Extension methods literally exist for this purpose.

---

# 🧠 **So what is an extension class, in simple words?**

> **A separate static class that adds new methods to an existing class
> without changing or touching the original class.**

---

# 🔥 When do developers use extension methods?

* After code is shipped, and you want to add small helper behavior
* To keep your original classes small and clean
* To extend 3rd-party classes you cannot modify
* To add utility functions to built-in types
* To avoid rewriting or duplicating logic

---

# 🧪 Example scenario (your exact case)

### BEFORE (class already in production):

```csharp
public class Person
{
    public string Name { get; set; }
}
```

### You now need:

`person.Name` → Capitalize it.

Instead of editing Person (risky), you do:

```csharp
public static class PersonExtensions
{
    public static string CapitalizedName(this Person p)
        => char.ToUpper(p.Name[0]) + p.Name.Substring(1);
}
```

Usage:

```csharp
person.CapitalizedName();
```

---

# 🎯 **Short Summary**

* **Extension class** = static class with extension methods
* **Used to add new functionality** without touching tested/production code
* **Safer**, cleaner, and follows **SOLID principles**
* **No risk** of breaking old behavior

---

If you want, I can show **10 useful real-world extension methods** used in enterprise apps.

