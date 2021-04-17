# Python2Aquila - Sepcial matches
import ast
from warning import warn
import expression_handler as expr_handler

def match_operator(operator : ast.operator) -> str:
	op_T = type(operator)
	# arithmetic
	if op_T is ast.Add:
		return '+'
	if op_T is ast.UAdd:
		return ''
	if op_T is ast.Sub or op_T is ast.USub:
		return '-'
	if op_T is ast.Mult:
		return '*'
	if op_T is ast.Div or op_T is ast.FloorDiv:
		return '/'
	if op_T is ast.Mod:
		return '%'
	# logic
	if op_T is ast.Lt:
		return '<'
	if op_T is ast.Gt:
		return '>'
	if op_T is ast.LtE:
		return '{'
	if op_T is ast.GtE:
		return '}'
	if op_T is ast.Eq:
		return '~'
	if op_T is ast.NotEq:
		return ':'
	if op_T is ast.Or:
		return '|' # no xor in python (not a logical one, only bitwise)
	if op_T is ast.And:
		return '&'

	warn('Unkown operator ' + str(operator))
	return '_OP_'

def match_function_call(call : ast.Call) -> (bool, str):
	# func name
	name = expr_handler.handle_expression(call.func, False)
	# match any known calls ?
	if name == 'print':
		#! value of string ? check
		if len(call.args) == 0:
			return True, 'print_endl()'
		arg = expr_handler.handle_expression(call.args[0])
		# str or value ?
		# str
		if type(call.args[0]) == ast.Constant and type(call.args[0].value) == str:
			return True, f'print_str_endl({arg})'
		# value
		return True, f'print_value_endl({arg})'
	if name == 'len':
		arg = call.args[0]
		return True, f'length({expr_handler.handle_expression(arg)})'
	if name == 'int':
		arg = call.args[0]
		return True, f'float2int({expr_handler.handle_expression(arg)})'
	if name == 'float':
		arg = call.args[0]
		return True, f'int2float({expr_handler.handle_expression(arg)})'

	return False, ''

def match_raw_value(value) -> str:
	value_str = str(value)
	value_T = type(value)
	if value_T is int:
		return value_str
	if value_T is float:
		return value_str + 'f'
	if value_T is bool:
		return value_str.lower()

	warn('Undefined value ' + value_str + (' (may be handled later)' if value_str == 'None' else ''))
	return value_str
