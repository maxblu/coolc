# Notes

We should put whatever we don't know where to put it but seems important (or not).

## AST
How do we find out how to make the **AST** and why do we think it's ok?

  > - What if *I need ASTs* not parse trees for a compiler, for example?  
  >
  > For writing a compiler, either generate [LLVM-type static-single-assignment](http://llvm.org/docs/LangRef.html) form or construct an AST from the parse tree using a listener or visitor. Or, use actions in grammar, **<u>turning off *auto-parse-tree* construction.</u>**

  Taken from: *Pragmatic.The.Definitive.ANLR.4.Reference.Jan.2013.pdf*

## Grammar syntax
    A grammar is essentially a grammar declaration followed by a list of rules but has the following general form:

    ```js
    /** Optional Javadoc-style comment */
    grammar Name;
    options {...}
    import ... ;
    tokens {...}
    @actionName {...}
    «rule1» // parser and lexer rules, possibly intermingled
    ...
    «ruleN»
    ```

* Keep an eye on the grammar 
  - i'm afraid that something blow out
    + it blow out
      * it fixed now

## Semantics
  
Need to do an *ast* class? with node types etc.?