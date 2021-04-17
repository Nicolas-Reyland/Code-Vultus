# Python2Aquila
import constants

def warn(s) -> None:
	if constants.VERBOSE:
		print('[!] WARNING:', s)
