package Node;

use strict;
use warnings;

sub new {
	my ($class) = @_;
	my $self = {
		_name => undef,
		_children => undef
	};
	$self->{_children} = ();
	bless $self, $class;
	return $self;
}

sub Name {
	my ($self, $x) = @_;
	$self->{_name} = $x if $x;
	return $self->{_name};
}

sub Children {
	my ($self, $x) = @_;
	$self->{_children} = $x if $x;
	return $self->{_children};
}

1;
