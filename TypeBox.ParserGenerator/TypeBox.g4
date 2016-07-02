grammar TypeBox;

/*
 * Parser Rules
 */

compileUnit
    : block EOF
    ;

block
    : blockItemList
    ;

lambdaExpression
	: 'function' '(' initDeclaratorList? ')' ':' typeSpecifier compoundStatement
	| '(' initDeclaratorList? ')' ':' typeSpecifier '=>' compoundStatement
	;

functionDeclaration
	: 'function' NAME '(' initDeclaratorList? ')' ':' typeSpecifier compoundStatement
	;

newExpression
	:	'new' typeSpecifier '(' argumentExpressionList? ')'
	;

primaryExpression
    :   NAME
    |   constant
//    |   StringLiteral+
    |   '(' expression ')'
	| lambdaExpression
	| newExpression
    ;

postfixExpression
    :   primaryExpression											# postfixPrimaryExpression
    |   postfixExpression '[' expression ']'						# arrayPostfixExpression
    |   postfixExpression '(' argumentExpressionList? ')'			# functionCall
    |   postfixExpression ('.' | '?.') NAME							# memberAccessExpression
    |   postfixExpression '++'										# postIncrementExpression
    |   postfixExpression '--'										# postDecrementExpression
    //|   '(' typeName ')' '{' initializerList '}'
    //|   '(' typeName ')' '{' initializerList ',' '}'
    ;

argumentExpressionList
    :   assignmentExpression
    |   argumentExpressionList ',' assignmentExpression
    ;

unaryExpression
    :   postfixExpression
    |   '++' unaryExpression
    |   '--' unaryExpression
    |   unaryOperator castExpression
//    |   'sizeof' unaryExpression
//    |   'sizeof' '(' typeName ')'
//    |   '_Alignof' '(' typeName ')'
//    |   '&&' Identifier // GCC extension address of label
    ;

unaryOperator
    :   '&' | '*' | '+' | '-' | '~' | '!'
    ;

castExpression
    :   unaryExpression
    |   '(' NAME ')' castExpression
//    |   '__extension__' '(' typeName ')' castExpression
    ;

multiplicativeExpression
    :   castExpression
    |   multiplicativeExpression '*' castExpression
    |   multiplicativeExpression '/' castExpression
    |   multiplicativeExpression '%' castExpression
    ;

additiveExpression
    :   multiplicativeExpression
    |   additiveExpression '+' multiplicativeExpression
    |   additiveExpression '-' multiplicativeExpression
    ;

shiftExpression
    :   additiveExpression
    |   shiftExpression '<<' additiveExpression
    |   shiftExpression '>>' additiveExpression
    ;

relationalExpression
    :   shiftExpression
    |   relationalExpression '<' shiftExpression
    |   relationalExpression '>' shiftExpression
    |   relationalExpression '<=' shiftExpression
    |   relationalExpression '>=' shiftExpression
    ;

equalityExpression
    :   relationalExpression
    |   equalityExpression '==' relationalExpression
    |   equalityExpression '!=' relationalExpression
    ;

andExpression
    :   equalityExpression
    |   andExpression '&' equalityExpression
    ;

exclusiveOrExpression
    :   andExpression
    |   exclusiveOrExpression '^' andExpression
    ;

inclusiveOrExpression
    :   exclusiveOrExpression
    |   inclusiveOrExpression '|' exclusiveOrExpression
    ;

logicalAndExpression
    :   inclusiveOrExpression
    |   logicalAndExpression '&&' inclusiveOrExpression
    ;

logicalOrExpression
    :   logicalAndExpression
    |   logicalOrExpression '||' logicalAndExpression
    ;

conditionalExpression
    :   logicalOrExpression ('?' expression ':' conditionalExpression)?
    ;

assignmentExpression
    :   conditionalExpression
    |   unaryExpression assignmentOperator assignmentExpression
    ;

assignmentOperator
    :   '=' | '*=' | '/=' | '%=' | '+=' | '-=' | '<<=' | '>>=' | '&=' | '^=' | '|='
    ;

expression
    :   assignmentExpression
    |   expression ',' assignmentExpression
    ;

expressionList
	: assignmentExpression
	| expressionList ',' assignmentExpression
	;

expression2
	: expression
	;

constantExpression
    :   conditionalExpression
    ;

variableDeclaration
	:   'var' initDeclaratorList
	;

declaration
    :	variableDeclaration ';'
	|	functionDeclaration
    ;

singleVariableDeclaration
	: 'var' declarator (':' typeSpecifier)?
	;

initDeclaratorList
    :   initDeclarator
    |   initDeclaratorList ',' initDeclarator
    ;

initDeclarator
    :   declarator
    |   declarator '=' initializer
	|   declarator ':' typeSpecifier
	|   declarator ':' typeSpecifier '=' initializer
    ;


typeSpecifier
	: basicType
	| basicType arraySpecifier
	| basicType '<' typeSpecifierList '>'
	;

typeSpecifierList
	: typeSpecifier
	| typeSpecifierList ',' typeSpecifier
	;

basicType
    :   ('void'
    |   'char'
    |   'short'
    |   'int'
    |   'long'
    |   'float'
    |   'double'
	|   'boolean'
	|   'number'
	|   'bool'
	|	'string'
	|	'Array')
	|	NAME
    ;

arraySpecifier
	: '[' ']'
	;

declarator
    :   NAME
    //|   '(' declarator ')'
    //|   directDeclarator '[' typeQualifierList? assignmentExpression? ']'
    //|   directDeclarator '[' 'static' typeQualifierList? assignmentExpression ']'
    //|   directDeclarator '[' typeQualifierList 'static' assignmentExpression ']'
    //|   directDeclarator '[' typeQualifierList? '*' ']'
    //|   directDeclarator '(' parameterTypeList ')'
    //|   directDeclarator '(' identifierList? ')'
	;

initializer
    :   assignmentExpression
    //|   '{' initializerList '}'
    //|   '{' initializerList ',' '}'
    ;


///////////////////////////////////////////////////////////////////

statement
    :   expressionStatement
//	labeledStatement
    |   compoundStatement
//    |   expressionStatement
    |   selectionStatement
    |   iterationStatement
    |   jumpStatement
    //|   ('__asm' | '__asm__') ('volatile' | '__volatile__') '(' (logicalOrExpression (',' logicalOrExpression)*)? (':' (logicalOrExpression (',' logicalOrExpression)*)?)* ')' ';'
    ;

//labeledStatement
//    :   Identifier ':' statement
//    |   'case' constantExpression ':' statement
//    |   'default' ':' statement
//    ;

compoundStatement
    :   '{' blockItemList? '}'
    ;

expressionStatement
    :   expression? ';'
    ;

selectionStatement
    :   'if' '(' expression ')' statement ('else' statement)?
//    |   'switch' '(' expression ')' statement
    ;

iterationStatement
	: 'for' '(' variableDeclaration? ';' expression? ';' expression2? ')' statement		# ForStatement
	| 'for' '(' singleVariableDeclaration 'of' assignmentExpression ')' statement		# ForOfStatement
    | 'while' '(' expression ')' statement										# WhileStatement
    | 'do' statement 'while' '(' expression ')' ';'								# DoWhileStatement
//    |   'for' '(' expression? ';' expression? ';' expression? ')' statement
//    |   'for' '(' declaration expression? ';' expression? ')' statement
    ;

jumpStatement
	: 'break' ';'
//    :   'goto' Identifier ';'
    |   'continue' ';'
    |   'return' expression? ';'
//    |   'goto' unaryExpression ';' // GCC extension
    ;

blockItemList
    :   blockItem
    |   blockItemList blockItem
    ;

blockItem
    :   declaration
    |   statement
    ;

arrayConstant
	: '[' expressionList? ']'
	;

constant
    : IntegerConstant
	| FloatConstant
	| BooleanConstant
	| arrayConstant
	| ObjectConstant
    ;

///////////////////////////////////////////////////////////////////

IntegerConstant
	: INT | HEX
	;

FloatConstant
	: FLOAT | HEX_FLOAT
	;

BooleanConstant
	: 'true' | 'false'
	;

ObjectConstant
	: 'null'
	;

NUMBER
    : INT | HEX | FLOAT | HEX_FLOAT
    ;

STRING
    : NORMALSTRING | CHARSTRING | LONGSTRING
    ;

/*
 * Lexer Rules
 */

NAME
    : [a-zA-Z_][a-zA-Z_0-9]*
    ;

NORMALSTRING
    : '"' ( EscapeSequence | ~('\\'|'"') )* '"' 
    ;

CHARSTRING
    : '\'' ( EscapeSequence | ~('\''|'\\') )* '\''
    ;

LONGSTRING
    : '[' NESTED_STR ']'
    ;

fragment
NESTED_STR
    : '=' NESTED_STR '='
    | '[' .*? ']'
    ;

INT
    : Digit+
    ;

HEX
    : '0' [xX] HexDigit+
    ;

FLOAT
    : Digit+ '.' Digit* ExponentPart?
    | '.' Digit+ ExponentPart?
    | Digit+ ExponentPart
    ;

HEX_FLOAT
    : '0' [xX] HexDigit+ '.' HexDigit* HexExponentPart?
    | '0' [xX] '.' HexDigit+ HexExponentPart?
    | '0' [xX] HexDigit+ HexExponentPart
    ;

fragment
ExponentPart
    : [eE] [+-]? Digit+
    ;

fragment
HexExponentPart
    : [pP] [+-]? Digit+
    ;

fragment
EscapeSequence
    : '\\' [abfnrtvz"'\\]
    | '\\' '\r'? '\n'
    | DecimalEscape
    | HexEscape
    ;
    
fragment
DecimalEscape
    : '\\' Digit
    | '\\' Digit Digit
    | '\\' [0-2] Digit Digit
    ;
    
fragment
HexEscape
    : '\\' 'x' HexDigit HexDigit
    ;

fragment
Digit
    : [0-9]
    ;

fragment
HexDigit
    : [0-9a-fA-F]
    ;

//COMMENT
//    : '--[' NESTED_STR ']' -> channel(HIDDEN)
//    ;
    
//LINE_COMMENT
//    : '--'
//    (                                               // --
//    | '[' '='*                                      // --[==
//    | '[' '='* ~('='|'['|'\r'|'\n') ~('\r'|'\n')*   // --[==AA
//    | ~('['|'\r'|'\n') ~('\r'|'\n')*                // --AAA
//    ) ('\r\n'|'\r'|'\n'|EOF)
//    -> channel(HIDDEN)
//    ;
    
WS  
    : [ \t\u000C\r\n]+ -> skip
    ;

SHEBANG
    : '#' '!' ~('\n'|'\r')* -> channel(HIDDEN)
    ;

BlockComment
    :   '/*' .*? '*/'
        -> skip
    ;

LineComment
    :   '//' ~[\r\n]*
        -> skip
    ;