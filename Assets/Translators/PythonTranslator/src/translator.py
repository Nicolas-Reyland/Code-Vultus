# Python2Aquila - CLI
import argparse, \
		constants, \
		os, \
		ast, \
		instruction_handler, \
		utils

# Argument Parser
parser = argparse.ArgumentParser(description='Translate basic python code into Aquila')
parser.add_argument('-i',
					'--infile',
					metavar='infile',
					type=str,
					help='Path to the python source code file that will be translated')
parser.add_argument('-o',
					'--outfile',
					metavar='outfile',
					type=str,
					help='Path to the Aquila source code file that will be generated')
parser.add_argument('-v',
					'--verbose',
					metavar='verbose',
					type=int,
					help='Verbose level [0, 1]')

def translate(infile : str, outfile : str, verbose = 1) -> None:
	# Set verbose
	constants.VERBOSE = verbose
	# Read source code
	infile_handler = open(infile, 'r')
	src_code = infile_handler.read()
	infile_handler.close()
	# Generate Aquila src code
	abstract_syntax_tree = ast.parse(src_code)
	generated_src_code = map(instruction_handler.handle_instruction, abstract_syntax_tree.body)
	# Write lines to outfile
	outfile_handler = open(outfile, 'w')
	new_src_code = utils.beautify_code(generated_src_code)
	outfile_handler.write(new_src_code)
	outfile_handler.close()
	# Confirmation
	print(f'Successfully written code to "{outfile}"')

if __name__ == '__main__':
	# Get the args
	args = vars(parser.parse_args())
	infile = args['infile']
	outfile = args['outfile']
	verbose = args['verbose'] if args['verbose'] else constants.VERBOSE

	# Check infile and outfile
	if not (infile and outfile):
		print('ERROR: You have to give the path to both the infile and outfile using "-i/--infile" and "-o/--outfile"')
		exit()
	# Paths
	if not os.path.isfile(infile):
		print(f'ERROR: infile - "{infile}" does not exist')
		exit()

	# Translate
	translate(infile, outfile, verbose)
