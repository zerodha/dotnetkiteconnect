import re
import xmltodict

DOC_XML = './kiteconnect.xml'
OUT_MD = './docs/reference.md'


def xml_to_map(xml_file):
    with open(xml_file, "rb") as f:    # notice the "rb" mode
        d = xmltodict.parse(f, xml_attribs=True)
        return d


md = open(OUT_MD, 'w')
xml = xml_to_map(DOC_XML)

for member in xml['doc']['members']['member']:
    # member has a format like "M:KiteConnect.Kite.CancelMFOrder(System.String)"
    # mtype is a single char that is defined as:
    # M -> method - function
    # T -> type - class
    # P -> public field
    # E -> event
    mtype, mname = member['@name'].split(':')
    argtypes = []

    # Hide delegates from docs. We will show only events
    if 'Handler' in mname and 'On' in mname:
        continue

    # remove namespace
    mname = mname.replace('KiteConnect.', '')
    mname = mname.split('(')

    # M means a function. collect argument types
    if mtype == 'M' and len(mname) > 1:
        mname[1] = re.sub(
            r'\Dictionary{(.*?),(.*?)\}', r'Dictionary{\1:\2}', mname[1])
        argtypes = mname[1].replace('System.', '').rstrip(')').split(',')

    # clean the name
    mname = mname[0]
    mname = mname.replace('.#ctor', ' Constructor')

    # write to markdown file
    if mtype == 'M':
        md.write(
            '### ![Method](/assets/method.jpg) &nbsp;&nbsp;' + mname + '\n\n')
    elif mtype == 'T':
        md.write(
            '## ![Class](/assets/class.jpg) &nbsp;&nbsp;' + mname + ' Class\n\n')
    elif mtype == 'P':
        md.write(
            '### ![Field](/assets/pubfield.jpg) &nbsp;&nbsp;' + mname + '\n\n')
    elif mtype == 'E':
        md.write(
            '### ![Event](/assets/event.jpg) &nbsp;&nbsp;' + mname + '\n\n')

    # write explanation
    md.write(member['summary'] + '\n\n')

    # if it is function and has params print as table
    if 'param' in member:
        md.write('| Argument | Type | Description |\n')
        md.write('| --- | --- | --- |\n')

        if type(member['param']) == list:
            for i in range(len(member['param'])):
                param = member['param'][i]
                md.write('| ' + param['@name'] + ' | ' + argtypes[i] +
                         ' | ' + param['#text'].replace('\n', '') + ' |\n')
        else:
            md.write('| ' + member['param']['@name'] + ' | ' + argtypes[0] +
                     ' | ' + member['param']['#text'].replace('\n', '') + ' |\n')

        md.write('\n')

    # write return param
    if 'returns' in member:
        md.write('**Returns:** ' + member['returns'] + '\n\n')
