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


primaryExpression
    :   NAME
    |   constant
//    |   StringLiteral+
    |   '(' expression ')'
    ;

postfixExpression
    :   primaryExpression
    |   postfixExpression '[' expression ']'
    |   postfixExpression '(' argumentExpressionList? ')'
    |   postfixExpression '.' NAME
    |   postfixExpression '++'
    |   postfixExpression '--'
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

constantExpression
    :   conditionalExpression
    ;

declaration
    :   typeSpecifier initDeclaratorList? ';'
    ;

initDeclaratorList
    :   initDeclarator
    |   initDeclaratorList ',' initDeclarator
    ;

initDeclarator
    :   declarator
    |   declarator '=' initializer
    ;

typeSpecifier
    :   ('void'
    |   'char'
    |   'short'
    |   'int'
    |   'long'
    |   'float'
    |   'double')
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
//    |   compoundStatement
//    |   expressionStatement
    //|   selectionStatement
    //|   iterationStatement
    //|   jumpStatement
    //|   ('__asm' | '__asm__') ('volatile' | '__volatile__') '(' (logicalOrExpression (',' logicalOrExpression)*)? (':' (logicalOrExpression (',' logicalOrExpression)*)?)* ')' ';'
    ;

//labeledStatement
//    :   Identifier ':' statement
//    |   'case' constantExpression ':' statement
//    |   'default' ':' statement
//    ;

//compoundStatement
//    :   '{' blockItemList? '}'
//    ;

expressionStatement
    :   expression? ';'
    ;

//selectionStatement
//    :   'if' '(' expression ')' statement ('else' statement)?
//    |   'switch' '(' expression ')' statement
//    ;

//iterationStatement
//    :   'while' '(' expression ')' statement
//    |   'do' statement 'while' '(' expression ')' ';'
//    |   'for' '(' expression? ';' expression? ';' expression? ')' statement
//    |   'for' '(' declaration expression? ';' expression? ')' statement
//    ;

//jumpStatement
//    :   'goto' Identifier ';'
//    |   'continue' ';'
//    |   'break' ';'
//    |   'return' expression? ';'
//    |   'goto' unaryExpression ';' // GCC extension
//    ;

blockItemList
    :   blockItem
    |   blockItemList blockItem
    ;

blockItem
    :   declaration
    |   statement
    ;


constant
    : IntegerConstant
	| FloatConstant
	| BooleanConstant
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