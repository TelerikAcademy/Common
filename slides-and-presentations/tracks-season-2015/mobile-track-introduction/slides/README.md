<!-- section start -->

<!-- attr: {id: 'title', class: 'slide-title', hasScriptWrapper: true} -->
#   C++ Introduction
##    What is C++, Features, Compilers, IDEs
<div class="signature">
    <p class="signature-course">C++ Fundamentals</p>
    <p class="signature-initiative">Telerik Algo Academy</p>
    <a href="http://academy.telerik.com" class="signature-link">http://academy.telerik.com</a>
</div>

<!-- section start -->

<!-- attr: {id: 'table-of-contents'} -->
#   Table of Contents

*   What is C++?
    *   History, Concepts & Philosophy, Standards
*   C++ Basic Program Structure
    *   Entry point, libraries, namespaces, etc...
*   C++ Compilers and IDEs
    *   Code::Blocks
    *   Visual Studio
    *   ViM
*   C++ Features
    *   C libs, OOP, Templates, Exceptions, Overloads

<!-- section start -->

<!-- attr: {id: 'what-is-cpp', class: 'slide-section', showInPresentation: true} -->
<!-- #   What is C++?
##    Fast, Mid-level, Multi-paradigm Language -->

#   What is C++?

*     **General purpose** programming language
    *   Any field of application - _science_, _business_, etc...
*   Compiles **to binary**
    *   Code directly executed on the hardware
*   Statically typed
    *   All data is in **predefined forms** (data types)
    *   Data is represented in variables of data types
    *   A variable is of only 1 data type through its lifespan
<!-- attr: {showInPresentation: true} -->
<!-- #   What is C++ (cont.)? -->
*   Multi-paradigm
    *   Supports **procedural programming** (as in C)
    *   Supports **object-oriented** programming
    *   Some **functional programming** in C++11 (latest)
*   Created by [Bjarne Stroustrup](http://www.stroustrup.com/)
    *   Originally _"C with Classes"_, later renamed
    *   Built over pure C, not fully compatible though

#   C++ Programming Model

*     **Imperative**, **Multi-paradigm** language
*   Programmer can mix and match
    *     **Low-level memory access** (down to each byte)
    *     **Procedural** code (functions, memory pointers, etc.)
    *     **Object-oriented** code (classes, methods, objects, etc.)

#   C++ Philosophy

*   Features immediately useful in the real world
*   Programmers free to pick their own style
*   Allowing useful features
    *   More important than preventing misuse
*   Features you do not use you do not pay for
*   Programmer can specify undefined behavior
*   More at http://en.wikipedia.org/wiki/C++#Philosophy


#   C++ Standards

*     **C++ 98** - first standardized C++ version
    *   Still massively used today
*     **C++ 03** - minor revision of 98, bug-fixes
*     **C++ TR1** - specification of extensions to be included in next C++ version
    *   Not really a standard
*     **C++ 11** - latest official revision
    *   Many new features and improvements
    *   Lambdas, range-based loops, etc.


<!-- section start -->

<!-- attr: {class: 'slide-section', id: 'cpp-program-structure', showInPresentation: true} -->
<!-- #   C++ Program Structure
##   Entry point, including libraries, termination -->

#   C++ Program Structure
*   Program entry point
    *   C++ is free form – any ordering of program components is acceptable
    *   C++ needs specific function to start from
*   The main function – entry point of the program
    *   No other function can be named "main"
    *   Can receive command line parameters
*   Termination – main returns, the program stops
    *   The return value of main is the "exit code“
    *   0 means no errors – informative, not obligatory

<!-- attr: {hasScriptWrapper: true} -->
#   C++ Classical "Hello World" Example

* This is a classical "Hello World" example in C++:

```cpp
#include <iostream>

using namespace std;

int main(int argc, char * argv[]) {

    cout<<"Hello World!"<<endl;

    return 0;
}
```

*   Include the input-output library <!-- .element: class="balloon" style="top: 30%; left:45%" -->
*   Say we're working with std namespace <!-- .element: class="balloon" style="top: 37%; left:45%" -->
*   main function - the entry point <!-- .element: class="balloon" style="top: 35%; left:45%" -->
*   Parameters in these brackets are optional <!-- .element: class="balloon" style="top: 35%; left:45%" -->
*   Print to the console <!-- .element: class="balloon" style="top: 35%; left:45%" -->
*   For main 0 means everything went OK, terminating normally <!-- .element: class="balloon" style="top: 35%; left:45%" -->

<!-- attr: {class: 'slide-section', showInPresentation: true} -->
<!-- #   C++ Hello World Example -->
##  Live Demo

<!-- section start -->

<!-- attr: {class: 'slide-section', id: 'cpp-ides-compilers', showInPresentation: true} -->
<!-- #   C++ IDEs, Editors and Compilers
##  Compiling code, Integrated Development Environments -->

#   C++ Compilers

*   A C++ compiler turns C++ code to assembly
    *   i.e. translates C++ to machine language
*   An IDE is software assisting programming
    *   Has a Compiler, Linker, Debugger, Code Editor
    *   Code organization, Tools, Diagnostics
There are lots of C++ compilers
    *   Free, open-source, proprietary
    *   Most are embedded in IDEs
    *   Bjarne Stroustrup’s advice on picking an IDE and compiler: http://stroustrup.com/compilers.html

<!-- attr: {id: 'code-blocks'}  -->
#  Code::Blocks

*     **Code::Blocks** - free C & C++ IDE
    *   Used in International Olympiad in Informatics
    *   Comes with MinGW GCC compiler
        *   Currently no support for C++ 11
*   Lightweight
    *   Can compile single `.cpp` file
    *   Can be usedfor bigger projects with many files, references, etc...

<!-- attr: {class:'slide-section', showInPresentation: true} -->
<!-- #   Code::Blocks
##    Live Demo -->

<!-- attr: {id: 'visual-studio'}  -->
# Visual Studio

*     **Visual Studio** – proprietary IDE for MS stack
    *   Supports latest C++ standards
*   Single tool for:
    *   Writing code in many languages (C#, C++, …)
    *   Using different technologies (Web, WPF, …)
    *   For different platforms (.NET CF, Silverlight, …)
    *   Full integration of most development activities (coding, compiling, testing, debugging, deployment, version control, ...)

<!-- attr: {class:'slide-section', showInPresentation: true} -->
<!-- #   Visual Studio
##    Live Demo
 -->
<!-- attr: {id: 'vim'}  -->
# VIM

* **VIM** (Vi IMproved)
	* CLI and GUI mode (gVim)
	* Highly configurable
	* Extensible through plugins
	* Can be used for all kinds of text editing
		* Not just programming
	* Tool, the use of which must be learned

<!-- attr: {class: 'slide-section', showInPresentation: true} -->
<!-- #   VIM
##    Live Demo -->

<!-- section start -->

<!-- attr: {class: 'slide-section', id: 'cpp-features', showInPresentation: true} -->
<!-- # C++ Features
##  Notable C++ Features, Supported in all Standards -->

# C++ Features

*   Operators and operator overloading
    *   Over 35 operators:
        *   Arithmetic, bitwise, comparisson, logical and more
    *   User types can redefine operators
*   Memory management
    *   Static allocation (compile-time, stack)
  *   Auto allocation (stack)
  *   Dynamic allocation (`new`, `delete`, heap)
<!-- attr: {showInPresentation: true} -->
<!-- # C++ Features -->
*   Classes & Objects
    *   Support for all OOP principles:
        *   inheritance, polymorphism, encapsulation, abstraction, virtuals
*   Templates
    *   Support for generic classes and methods
<!-- attr: {showInPresentation: true} -->
<!-- # C++ Features -->
*   Standard Library
    *   Set of libraries, data structures and algorithms
    *   Largely based on the [STL](https://en.wikipedia.org/wiki/Standard_Template_Library)
*   Exceptions
    *   Objects representing errors
    *   Can interrupt control flow and propagate to handlers
    *   Can be user-created

<!-- section start -->

<!-- attr: {id: 'questions', class:'slide-questions'} -->
# Introduction to C++
##  Questions
