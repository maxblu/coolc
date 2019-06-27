# Coolc
**A cool compiler for a cool language**

## Authors

[Daniel de la Osa Fernandez](d.osa@estudiantes.matcom.uh.cu)
[Jose Luis Alvarez de la Campa](j.alvarez@estudiantes.matcom.uh.cu)

## Introduction

This is a compiler for the **COOL** programming languaje (**C**lassromm **O**bject **O**riented **L**anguage). This project is part of the **Complements of Compilers** course in **University Of Havana**. 

## Requirements

* **MSBuild.exe** added to your path.
* **Antlr4.Runtime.Standard.dll**: the standard library.
* **antlr-4.7-complete.jar**: the java binary to generate csharp code
* **Java(TM) SE Runtime Environment (build 1.8.0-ea-b77)** added to your path to regenerate the *ANTLR* output.

## Project structure

* **bin/**
  - it's where all the binaries are (the compiler)
* **doc/**
  - it's where all the documentation are
    + Notes
    + TODO
    + LOG
* **src/**
  - it's where all the code are
    + (VS project)
* **test/**
  - cool programs to be compiled
* **tools/**
  - here is where antlr, csc and maybe java should go
    + (this is for later... way later)
* **build.bat**
  - this guy run all the compiling process 
* **test.bat**
  - the test process automated

## Building

On Windows: Use the **build.bat**

## Use

To compile a Cool program type:

```bash
coolc.exe <filename.cl>
```

The compiler produces MIPS assembly code.

To “execute” the program use the SPIM simulator:

```bash
spim.exe –file <filename.s>
```

