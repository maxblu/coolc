grammar Cool; 

import CoolLexerRules; // includes all rules from CommonLexerRules.g4

@actionName 
{
	auto-parse-tree=off; //this isn't doing anything
}

@header {


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
#pragma warning disable CS0105 //The using directive for 'System.Collections.Generic' appeared previously in this namespace [D:\Desktop\projects\coolc\coolc\src\coolc\coolc.csproj

}
@members{

}

program : (classdef SEMICOLON)+ EOF
	;
classdef : CLASS t=TYPE (INHERITS it=TYPE)? LBRACE (feature SEMICOLON)* RBRACE
	;
feature : id=ID LPAREN (formal (COMMA formal)*)? RPAREN COLON t=TYPE LBRACE e=expresion RBRACE							#methodFeature
	| id=ID COLON t=TYPE (ASSIGN e=expresion)?																			#attribFeature
	;
formal : id=ID COLON t=TYPE
	;
expresion : expresion (ATSYM t=TYPE)? DOT id=ID LPAREN(expresion (COMMA expresion)*)?RPAREN								#atsimExp
	| id=ID LPAREN (expresion (COMMA expresion)* )? RPAREN
	{
		
		}																#methodCallExp
	| IF expresion THEN expresion ELSE expresion FI																		#ifExp	
	| WHILE le=expresion LOOP re=expresion POOL																			#whileExp
	| LBRACE (expresion SEMICOLON)+ RBRACE																				#bracedExp
	| LET newvar (COMMA newvar)* IN body=expresion																		#letExp
	| CASE expresion OF (ID COLON t=TYPE DARROW expresion SEMICOLON)+ ESAC												#caseExp
	| NEW t=TYPE																										#newTypeExp
	| ISVOID e=expresion																								#isvoidExp
	| le=expresion op=(STAR|SLASH) re=expresion																			#infixExp
	| le=expresion op=(PLUS|MINUS) re=expresion																			#infixExp
	| TILDE e=expresion																									#tildeExp
	| le=expresion LT re=expresion																						#lessExp
	| le=expresion LE re=expresion																						#lessEqualExp
	| le=expresion EQUALS re=expresion																					#equalsExp
	| NOT e=expresion																									#notExp
	| LPAREN e=expresion RPAREN																							#parentExp
	| s=STRING																											#stringExp
	| i=INT																												#intExp
	| BOOL																												#boolExp
	| id=ID																												#identifierExp
	| id=ID ASSIGN e=expresion																							#assignExp
	;

newvar : id=ID COLON t=TYPE (ASSIGN e=expresion)?
	;
//| (ID | BOOL | INT | STRING )																						#constantExp
