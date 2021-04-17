
GREETING_MSG = '/** Automatic translation of Python source code to Aquila by https://github.com/Nicolas-Reyland/Python2Aquila **/'

PY_SETTINGS = '''/** Python default settings **/
#setting (interactive) false
#setting (debug) false
#setting (trace debug) false
#setting (translator debug) false
#setting (fail on context assertions) false
#setting (check function existence before runtime) false
#setting (lazy logic) true
#setting (allow tracing in frozen context) true
#setting (permafrost) false
#setting (flame mode) true
#setting (implicit declaration in assignment) true
#setting (redirect debug stout & stderr) false'''

PY_FUNCTIONS = '''/** Python functions in Aquila **/
function list range(start, stop, step)
	decl list l []
	for (decl i $start, $i < $stop, $i += $step)
		append_value($l, $i)
	end-for
	return($l)
end-function

function list _py_slice_list(l, start, stop, step)
	decl list spliced_list []
	for ($i = $start, $i < $stop, $i += $step)
		append_value($spliced_list, list_at($l, $i))
	end-for
	return($spliced_list)
end-function'''

VERBOSE = 1

FORBIDDEN_WORDS = ['if', 'else', 'end-if',
					'for','end-for',
					'while', 'end-while',
					'function', 'end-function', 'recursive',
					'decl', 'safe', 'overwrite',
					'trace',
					'null', 'auto', 'int', 'float', 'bool', 'list',]
