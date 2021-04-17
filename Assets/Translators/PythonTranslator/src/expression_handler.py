import ast
from constants import FORBIDDEN_WORDS
from special_matches import match_operator, match_function_call, match_raw_value
from warning import warn

def handle_expression(expression : ast.AST, var_prefix = True) -> str:
	expr_T = type(expression)
	# values
	if expr_T is ast.Constant:
		return match_raw_value(expression.value)
	if expr_T is ast.Name:
		return handle_name(expression, var_prefix)
	if expr_T is ast.List:
		return handle_list(expression)
	if expr_T is ast.UnaryOp:
		return handle_unary_operator(expression)
	# arith & logic operations
	if expr_T is ast.BinOp:
		return handle_bin_op(expression)
	if expr_T is ast.BoolOp:
		bool_op = handle_bool_op(expression)
		# unique case
		bool_op = bool_op.replace('$__name__ ~ __main__', 'true')
		bool_op = bool_op.replace('__main__ ~ $__name__', 'true')
		# return
		return bool_op
	if expr_T is ast.Compare:
		return handle_comparaison(expression)
	# function call
	if expr_T is ast.Call:
		return handle_call(expression)
	# subscript & slices
	if expr_T is ast.Subscript:
		return handle_subscript(expression)
	# attributes
	if expr_T is ast.Attribute:
		return handle_attribute(expression)

	# expression within
	if expr_T is ast.Expr:
		return handle_expression(expression.value)
	# a raw value ?
	if expr_T is not ast.AST:
		return match_raw_value(expression)

	print('unkown:', expr_T)
	return str(expression)

def handle_name(name_expr : ast.Name, var_prefix : bool) -> str:
	name = str(name_expr.id)
	# forbidden words
	if name in FORBIDDEN_WORDS:
		name += '_'
	# var prefix (function names dont take any prefix)
	if var_prefix:
		name = '$' + name

	return name

def handle_list(list : ast.List):
	return f"[{', '.join(map(handle_expression, list.elts))}]"

def handle_bin_op(bin_op : ast.BinOp) -> str:
	return f'({handle_expression(bin_op.left)} {match_operator(bin_op.op)} {handle_expression(bin_op.right)})'

def handle_bool_op(bool_op : ast.BoolOp) -> str:
	bool_op_str = handle_expression(bool_op.values[0])
	op = match_operator(bool_op.op)
	for value in map(handle_expression, bool_op.values[1:]):
		bool_op_str += f' {op} {value}'
	return '(' + bool_op_str + ')'

def handle_call(call : ast.Call):
	# special function case ?
	special, s = match_function_call(call)
	if special: return s
	# normal case
	name = handle_expression(call.func, False)
	args = ', '.join(map(handle_expression, call.args))

	return f'{name}({args})'

def handle_comparaison(comparaison : ast.Compare) -> str:
	comp_str = handle_expression(comparaison.left)
	num_ops = len(comparaison.comparators)
	#
	for i in range(num_ops):
		# add this comparaison
		comparator = handle_expression(comparaison.comparators[i])
		comp_str += ' ' + match_operator(comparaison.ops[i]) + ' '
		comp_str += comparator
		# don't do this at the end
		if i != num_ops - 1:
			comp_str += ' & ' + comparator

	return '(' + comp_str + ')'

def handle_unary_operator(unary : ast.UnaryOp) -> str:
	op = match_operator(unary.op)
	operand = handle_expression(unary.operand)
	return op + '(' + operand + ')'

def handle_attribute(attribute : ast.Attribute) -> str:
	return f'{handle_expression(attribute.value)}.{attribute.attr}'

# list comprehension
def handle_subscript(subscript : ast.Subscript) -> str:
	value = handle_expression(subscript.value)
	# slice expression
	if type(subscript.slice) is ast.Slice:
		return _handle_slice(value, subscript.slice)
	# normal index
	slice_ = handle_expression(subscript.slice)
	return f'{value}[{slice_}]'

def _handle_slice(list_expr : str, slice_expr : ast.Slice) -> str:
	# get the expressions
	start = handle_expression(slice_expr.lower)
	if start == 'None': start = '0'
	stop = handle_expression(slice_expr.upper)
	if stop == 'None': stop = f'length({list_expr})'
	step = handle_expression(slice_expr.step)
	if step == 'None': step = '1'
	# slice it
	return f'_py_slice_list({list_expr}, {start}, {stop}, {step})'
