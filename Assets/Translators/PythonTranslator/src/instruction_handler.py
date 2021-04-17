# Handle the instructions
import ast
import expression_handler as expr_handler
from warning import warn

def handle_instruction(instruction : ast.AST) -> str:
	T = type(instruction)
	if T is ast.Expr:
		return expr_handler.handle_expression(instruction)
	# Assignments
	if T is ast.Assign:
		return handle_assignment(instruction)
	if T is ast.AugAssign:
		return handle_aug_assignment(instruction)
	# Nested instructions
	if T is ast.For:
		return handle_for(instruction)
	if T is ast.While:
		return handle_while(instruction)
	if T is ast.If:
		return handle_if(instruction)
	# Functions
	if T is ast.FunctionDef:
		return handle_function_def(instruction)
	if T is ast.Return:
		return handle_return(instruction)
	
	warn('Unkown instruction' + str(instruction))
	return 'unknown ' + str(instruction)

def handle_assignment(assignment : ast.Assign) -> str:
	var_names = ' = '.join(map(expr_handler.handle_expression, assignment.targets))
	value = expr_handler.handle_expression(assignment.value)
	legal = len(assignment.targets) == 1
	if not legal: warn('Illegal code created -> multiple variable assignments at once')
	return f'{var_names} = {value}' + ('' if legal == 1 else ' /** this is not legal **/')

def handle_aug_assignment(assignment : ast.AugAssign) -> str:
	var_name = expr_handler.handle_expression(assignment.target)
	# get value str
	value = expr_handler.handle_expression(assignment.value)
	# operator
	op = expr_handler.match_operator(assignment.op)
	return f'{var_name} {op}= {value}'

def handle_function_def(function_def : ast.FunctionDef) -> str:
	args_string = ', '.join(map(lambda x: x.arg, function_def.args.args))
	func_body_str = '\n'.join(map(handle_instruction, function_def.body))
	func_str = f'function recursive auto {function_def.name}({args_string})\n' + \
			func_body_str + '\n' + \
			'end-function'
	return func_str

def handle_for(for_loop : ast.For) -> str:
	# check for range loop
	is_range, for_loop_equivalent = _handle_range_for_loop(for_loop)

	# python for loops are exclusively iterator loops. Can somehow work with that, but is difficult
	for_loop_body = '\n'.join(map(handle_instruction, for_loop.body))
	iterator = expr_handler.handle_expression(for_loop.iter)
	iterator_var = expr_handler.handle_expression(for_loop.target)

	# custom for_loop_equivalent
	if not is_range:
		for_loop_equivalent = \
			'// Generating an python for-loop equivalent\n' + \
			f'$_py_for_loop_iterator = copy_list({iterator})\n' + \
			f'$_py_for_loop_num_iterations = length($_py_for_loop_iterator)\n' + \
			'for ($_py_for_loop_index = 0, $_for_loop_index < $_py_for_loop_num_iterations, $_py_for_loop_index ++)\n' + \
				f'{iterator_var} = $_py_for_loop_iterator[$_py_for_loop_index]\n' + \
				'// start of actual translated code in loop\n'

	return for_loop_equivalent + for_loop_body + '\nend-for'

def _is_corresponding_char(s : str, opening : str, closing : str) -> bool:
	depth = 0
	for i in range(len(s)):
		c = s[i]
		if c == opening:
			depth += 1
		if c == closing:
			depth -= 1
			if depth == 0:
				return i == len(s) - 1
	return False

def _handle_range_for_loop(for_loop : ast.For) -> (bool, str):
	# check the iterator for a valid 'range'
	iterator = expr_handler.handle_expression(for_loop.iter)
	iterator_var = expr_handler.handle_expression(for_loop.target)

	if type(for_loop.iter) is not ast.Call: return False, ''
	if not iterator.startswith('range('): return False, ''
	if not _is_corresponding_char(iterator, '(', ')'): return False, ''

	# get the range args
	args = for_loop.iter.args
	start, stop, step = 0, None, 1
	n = len(args)
	if n == 0 or n > 3: return False, ''
	if n == 1:
		stop = expr_handler.handle_expression(args[0])
	elif n == 2:
		start = expr_handler.handle_expression(args[0])
		stop = expr_handler.handle_expression(args[1])
	else:
		start = expr_handler.handle_expression(args[0])
		stop = expr_handler.handle_expression(args[1])
		step = expr_handler.handle_expression(args[2])

	# build for_loop
	return True, f'for ({iterator_var} = {start}, {iterator_var} < {stop}, {iterator_var} += {step})\n'

def handle_while(while_loop : ast.While) -> str:
	while_loop_body = '\n'.join(map(handle_instruction, while_loop.body))
	test = expr_handler.handle_expression(while_loop.test)
	# don't need parentheses for the while condition bc the logic operations add a pair eaach time
	return f'while {test}\n{while_loop_body}\nend-while'

def handle_if(if_else : ast.If) -> str:
	test = expr_handler.handle_expression(if_else.test)
	# __name__ == '__main__' special case
	test = test.replace('$__name__ ~ __main__', 'true')
	test = test.replace('__main__ ~ $__name__', 'true')
	# if block
	if_else_body = '\n'.join(map(handle_instruction, if_else.body))
	# else block
	if if_else.orelse:
		if_else_body += '\nelse\n' + '\n'.join(map(handle_instruction, if_else.orelse))

	# don't need parentheses for the if condition bc the logic operations add a pair eaach time
	return f'if {test}\n{if_else_body}\nend-if'

def handle_return(return_call : ast.Return) -> str:
	value = expr_handler.handle_expression(return_call.value)
	return f'return({value})'
