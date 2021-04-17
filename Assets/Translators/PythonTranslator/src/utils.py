# Python2Aquila - utils
import constants

opening_words = ['function ', 'for ', 'while ', 'if ', 'else']
closing_words = ['end-function', 'end-for', 'end-while', 'end-if', 'else']

def beautify_code(lines : list) -> str:
	src_code = ''
	depth = 0
	full = '\n'.join(lines)
	for line in full.split('\n'):
		for closing in closing_words:
			if line == closing:
				depth -= 1
		src_code += '\t' * max(depth, 0) + line + '\n'
		for opening in opening_words:
			if line.startswith(opening):
				depth += 1
		if line == 'end-function':
			src_code += '\n'
	return constants.GREETING_MSG + '\n\n' + \
		constants.PY_SETTINGS + '\n\n' + \
		constants.PY_FUNCTIONS + '\n\n' + \
		'/** Source Code **/\n' + \
		src_code
