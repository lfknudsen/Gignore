# Gignore

Generates a .gitignore based on a supplied list of template names.\
The programme calls TopTal's API under the hood, which in turn uses GitHub's .gitignore templates.

## Dependencies

Requires .NET 10.

## Setup

Execute the following command to build the programme:

```
dotnet publish src -o <output directory>
```

Be aware that a .NET programme consists of an entire directory, not just an executable.
As such, the output directory should not be one that already exists (unless 
you're re-compiling).

Add the output directory to your PATH.

## Usage

#### Compile
To compile a .gitignore in your current directory, execute the following command,
adding as many template names as you wish.
```
gignore [--skip-check] <template names...>
```

If `--skip-check`, the validity of the template names is not checked before the API is called.

#### List

To see the full list of available template names (you may want to use a pager like `less`),
execute instead
```
gignore list
```

#### Search

To search for any template names that contain particular search terms, and have any matches
printed to standard output, execute
```
gignore search <search terms>
```


